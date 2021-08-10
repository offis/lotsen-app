// Copyright (c) 2021 OFFIS e.V.. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//    
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//    
// 3. Neither the name of the copyright holder nor the names of its contributors
//    may be used to endorse or promote products derived from this software without
//    specific prior written permission.
//    
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using MoreLinq;

namespace LotsenApp.LicenseManager.DependencyCrawling.Nuget
{
    public class CsProjCrawler: IProjectCrawler
    {
        private readonly NugetService _nugetService;
        private readonly NugetCache _cache;
        public string ApplicableFileExtension => "csproj";

        
        
        public CsProjCrawler(NugetService nugetService, NugetCache cache)
        {
            _nugetService = nugetService;
            _cache = cache;
        }

        public async Task<DependencyInformation[]> GetInformation(string file)
        {
            var fileInfo = new FileInfo(file);
            var doc = new XmlDocument();
            doc.Load(file);
            var nodes = doc.DocumentElement?.ChildNodes.Cast<XmlNode>();
            var children = nodes?.SelectMany(n => n.ChildNodes.Cast<XmlNode>());
            var references = children?.Where(n => n.Name == "PackageReference");
            var directDependencies = references?.Select(r => 
                GetDirectDependencyInformation(fileInfo.Directory?.FullName, 
                    r.Attributes?.Cast<XmlAttribute>().First(n => n.Name == "Include").InnerText,
                    r.Attributes?.Cast<XmlAttribute>().First(n => n.Name == "Version").InnerText));
            var awaitedDependencies = await Task.WhenAll(directDependencies ?? Enumerable.Empty<Task<DependencyInformation>>());
            var transitiveDependencies = awaitedDependencies.Select(GetTransitiveDependencies);
            var resolvedDependencies = await Task.WhenAll(transitiveDependencies);
            var flattenedDependencies = resolvedDependencies.SelectMany(d => d);
            return MoreEnumerable
                .DistinctBy(awaitedDependencies
                .Concat(flattenedDependencies), d => (d.DependencyName, d.DependencyVersion))
                .ToArray();
        }

        public async Task<DependencyInformation[]> GetTransitiveDependencies(DependencyInformation information)
        {
            if (_cache.TryGetCached(information.DependencyName, information.DependencyVersion, out var cachedEntry))
            {
                return await ResolveDependencies(cachedEntry);
            }

            try
            {
                var resource = await _nugetService.GetMetadataServiceUrl();
                var registration = await _nugetService.GetRegistrationEntry(resource, information.DependencyName,
                    information.DependencyVersion);
                var catalog = await _nugetService.GetCatalogEntry(registration);
                _cache.Cache(information.DependencyName, information.DependencyVersion, catalog);
                return await ResolveDependencies(catalog);
            }
            catch (Exception)
            {
                Console.WriteLine($"The entry for {information.DependencyName}@{information.DependencyVersion} could not be found.");
            }

            return new []{information};
        }

        public async Task<DependencyInformation[]> ResolveDependencies(NugetCatalogEntry entry)
        {
            var ownDependency = CreateFromCatalogEntry(entry);
            if (!entry.DependencyGroups.Any())
            {
                return new []{ownDependency};
            }
            var allDependencies = entry.DependencyGroups.Last().Dependencies.Select(async d =>
            {
                try
                {
                    var catalogEntry = await _nugetService.GetDependencyInformation(d);
                    var transitiveInformation = CreateFromCatalogEntry(catalogEntry);
                    var nextLevelInformation = await GetTransitiveDependencies(transitiveInformation);
                    return Enumerable.Prepend(nextLevelInformation, transitiveInformation);
                }
                catch (Exception)
                {
                    Console.WriteLine($"A dependency {d.Id} could not be resolved");
                }

                return new DependencyInformation[0];
            });
            var allResolvedDependencies = await Task.WhenAll(allDependencies);
            return Enumerable.Prepend(allResolvedDependencies.SelectMany(d => d), ownDependency)
                .ToArray();
        }

        public DependencyInformation CreateFromCatalogEntry(NugetCatalogEntry entry)
        {
            return new DependencyInformation
            {
                Origin = "Transitive Dependency",
                DependencyName = entry.Id,
                DependencyVersion = entry.Version,
                DependencyType = DependencyType.Nuget,
                DirectDependency = false,
                RepositoryUrl = entry.Repository ?? entry.ProjectUrl
            };
        }
        
        public async Task<DependencyInformation> GetDirectDependencyInformation(string origin, string dependency, string version)
        {
            NugetCatalogEntry entry;
            _cache.TryGetCached(dependency, version, out entry);
            if (entry == null)
            {
                var resource = await _nugetService.GetMetadataServiceUrl();
                var registration = await _nugetService.GetRegistrationEntry(resource, dependency,
                    version);
                var catalog = await _nugetService.GetCatalogEntry(registration);
                _cache.Cache(dependency, version, catalog);
                entry = catalog;
            }
            var repository = entry?.Repository == null || entry.Repository == "" ? 
                    entry?.ProjectUrl : 
                    entry.Repository;
            return new DependencyInformation
            {
                Origin = origin,
                DependencyName = dependency,
                DependencyVersion = version,
                DependencyType = DependencyType.Nuget,
                DirectDependency = true,
                RepositoryUrl =  repository
            };
        }
    }
}