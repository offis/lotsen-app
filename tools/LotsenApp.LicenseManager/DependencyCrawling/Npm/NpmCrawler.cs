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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Newtonsoft.Json.Linq;

namespace LotsenApp.LicenseManager.DependencyCrawling.Npm
{
    public class NpmCrawler: IProjectCrawler
    {
        public string ApplicableFileExtension => "package.json";
        
        public Task<DependencyInformation[]> GetInformation(string file)
        {
            var fileInfo = new FileInfo(file);
            var origin = fileInfo.Directory;
            var nodeModules = Path.Join(origin?.FullName, "node_modules");
            var nodeModulesInfo = new DirectoryInfo(nodeModules);
            if (!nodeModulesInfo.Exists)
            {
                return Task.FromResult(Array.Empty<DependencyInformation>());
            }
            var modules = CrawlNodeModules(nodeModulesInfo, origin?.FullName);
            var distinctModules = MoreEnumerable
                .DistinctBy(modules, m => (m.DependencyName, m.DependencyVersion))
                .ToArray();
            // Distinguish direct dependencies
            var directDependencies = GetDirectDependencies(file)
                .ToDictionary(k => $"{k.DependencyName}@{k.DependencyVersion}");
            var distinctModulesWithDirectDependencies = distinctModules.Select(d =>
            {
                d.DirectDependency = directDependencies.ContainsKey($"{d.DependencyName}@{d.DependencyVersion}");
                return d;
            }).ToArray();
            return Task.FromResult(distinctModulesWithDirectDependencies);
        }

        public DependencyInformation[] GetDirectDependencies(string file)
        {
            var fileInfo = new FileInfo(file);
            var content = File.ReadAllText(file);
            var projectInformation = JObject.Parse(content);
            var dependencies = projectInformation["dependencies"];
            var devDependencies = projectInformation["devDependencies"];
            var optionalDependencies = projectInformation["optionalDependencies"];
            var dependencyInformation = dependencies?.Children()
                .Select(c => (JProperty) c)
                .Select(p => CreateDependency(p, fileInfo.Directory?.FullName));
            var devDependencyInformation = devDependencies?.Children()
                .Select(c => (JProperty) c)
                .Select(p => CreateDependency(p, fileInfo.Directory?.FullName));
            var optionalDependencyInformation = optionalDependencies?.Children()
                .Select(c => (JProperty) c)
                .Select(p => CreateDependency(p, fileInfo.Directory?.FullName));

            return dependencyInformation?
                .Concat(devDependencyInformation ?? Enumerable.Empty<DependencyInformation>())
                .Concat(optionalDependencyInformation ?? Enumerable.Empty<DependencyInformation>())
                .ToArray() ?? Array.Empty<DependencyInformation>();
        }
        
        public DependencyInformation[] CrawlNodeModules(DirectoryInfo directoryInfo, string origin)
        {
            var packageJson = directoryInfo
                .GetFiles()
                .FirstOrDefault(f => f.FullName.EndsWith("package.json"));
            DependencyInformation information = null;
            if (packageJson != null)
            {
                var content = File.ReadAllText(packageJson.FullName);
                var projectInformation = JObject.Parse(content);
                try
                {
                    var name = projectInformation["name"]?.ToString();
                    var version = projectInformation["version"]?.ToString();
                    // var license = projectInformation["license"]?.ToString();
                    var homepage = projectInformation["homepage"]?.ToString();
                    var repository = projectInformation["repository"];
                    var repositoryUrl = repository?.GetType() == typeof(JValue) ? 
                        repository?.ToString() : 
                        projectInformation["repository"]?["url"]?.ToString();
                    information = new DependencyInformation
                    {
                        Origin = directoryInfo.FullName,
                        DependencyName = name,
                        DependencyType = DependencyType.Npm,
                        DependencyVersion = version,
                        RepositoryUrl = repositoryUrl ?? homepage,
                    };
                }
                catch (Exception)
                {
                    Debug.WriteLine($"The file {packageJson} could not be read properly");
                }

            }
            
            return Enumerable.Prepend(directoryInfo
                    .GetDirectories()
                    .Where(d => d.Name != "node_modules")
                    .SelectMany(d => CrawlNodeModules(d, origin)), information)
                .Where(d => d?.DependencyName != null)
                .ToArray();
        }

        private DependencyInformation CreateDependency(JProperty property, string directory)
        {
            var version = property
                .Value
                .ToString()
                .Replace("~", "")
                .Replace("^", "");
            return new DependencyInformation
            {
                Origin = directory,
                DependencyName = property.Name,
                DependencyVersion = version,
                DependencyType = DependencyType.Npm
            };
        }
    }
}