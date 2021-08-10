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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LotsenApp.LicenseManager.Configuration;
using LotsenApp.LicenseManager.DependencyCrawling;
using MoreLinq;
using MoreLinq.Extensions;
using Newtonsoft.Json;

namespace LotsenApp.LicenseManager.LicenseResolving
{
    public class LicenseResolver
    {
        private const string CacheFile = "licenses.json";
        private const string AssociationFile = "licenses-association.json";
        private readonly ILicenseResolver[] _resolvers;
        private readonly LicenseService _licenseService;
        private readonly LicenseManagerConfiguration _configuration;
        private IDictionary<string, LicenseInformation> _cache = new Dictionary<string, LicenseInformation>();
        private ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();
        private string CacheFileLocation => Path.Join(_configuration.CacheFolder, CacheFile);
        private string AssociationFileLocation => Path.Join(_configuration.CacheFolder, AssociationFile);

        public LicenseResolver(IEnumerable<ILicenseResolver> resolvers, LicenseService licenseService, LicenseManagerConfiguration configuration)
        {
            _resolvers = resolvers.ToArray();
            _licenseService = licenseService;
            _configuration = configuration;
            ReadFileCache();
        }

        public async Task<string> ResolveLicenseForDependency(DependencyInformation dependency)
        {
            var resolver = _resolvers.First(r => r.SupportedDependencyType == dependency.DependencyType);
            var sdpx = await resolver.GetSpdxIdentifier(dependency);
            return sdpx;
        }
        
        public async Task<List<(DependencyInformation, string)>> ResolveLicensesForDependencies(DependencyInformation[] dependencies)
        {
            var uniqueDependencies = MoreEnumerable.DistinctBy(dependencies, d => (d.DependencyName, d.DependencyVersion));
            var dep = new List<(DependencyInformation, string)>();
            foreach (var dependencyInformation in uniqueDependencies)
            {
                var spdxIdentifier = await ResolveLicenseForDependency(dependencyInformation);
                dep.Add((dependencyInformation, spdxIdentifier));
            }

            return dep;
        }

        public Task<LicenseInformation> GetLicenseInformation(string spdxIdentifier)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                
                Console.WriteLine("Attempting to get license for " + spdxIdentifier);
                if (spdxIdentifier == null)
                {
                    Console.WriteLine("Return null  for no license");
                    return Task.FromResult<LicenseInformation>(null);
                }

                if (_cache.ContainsKey(spdxIdentifier))
                {
                    Console.WriteLine("Returning license from cache " + spdxIdentifier);
                    return Task.FromResult(_cache[spdxIdentifier]);
                }
                var retrievalTask = _licenseService.GetLicenseInformation(spdxIdentifier);
                retrievalTask.Wait();
                var information = retrievalTask.Result;
                Console.WriteLine("Got information for " + spdxIdentifier);
                UpdateLicenseCache(information);
                Console.WriteLine("Added " + spdxIdentifier + " to cache");
                return Task.FromResult(information);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception of any kind occurred: {ex.Message}\n{ex.StackTrace}");
                throw new Exception("An exception occurred during execution", ex);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
            
        }

        public void ReadFileCache()
        {
            if (File.Exists(CacheFileLocation))
            {
                var content = File.ReadAllText(CacheFileLocation);
                var licenses = JsonConvert.DeserializeObject<List<LicenseInformation>>(content);
                _cache = licenses.ToDictionary(k => k.LicenseId, v => v);
            }
        }

        public async Task AddLicenseToCache(LicenseInformation model)
        {
            var licenses = new List<LicenseInformation>();
            if (File.Exists(CacheFileLocation))
            {
                var content = await File.ReadAllTextAsync(CacheFileLocation);
                licenses = JsonConvert.DeserializeObject<List<LicenseInformation>>(content);
            }

            if (licenses.Any(l => l.LicenseId == model.LicenseId))
            {
                throw new Exception($"A license with the identifier {model.LicenseId} already exists");
            }

            licenses.Add(model);
            _cache.TryAdd(model.LicenseId, model);

            var serializedLicenses = JsonConvert.SerializeObject(licenses, Formatting.Indented);

            await File.WriteAllTextAsync(CacheFileLocation, serializedLicenses);
        }
        
        public void UpdateLicenseCache(LicenseInformation model)
        {
            var licenses = new List<LicenseInformation>();
            if (File.Exists(CacheFileLocation))
            {
                var content = File.ReadAllText(CacheFileLocation);
                licenses = JsonConvert.DeserializeObject<List<LicenseInformation>>(content);
            }

            
            licenses = licenses.Where(l => l.LicenseId != model.LicenseId).ToList();
            licenses.Add(model);
            if (_cache.ContainsKey(model.LicenseId))
            {
                _cache[model.LicenseId] = model;                
            }
            else
            {
                _cache.Add(model.LicenseId, model);
            }

            

            var serializedLicenses = JsonConvert.SerializeObject(licenses, Formatting.Indented);

            File.WriteAllText(CacheFileLocation, serializedLicenses);
        }
        
        public async Task<IEnumerable<LicenseInformation>> Crawl(IEnumerable<DependencyInformation> dependencies)
        {
            var dependenciesWithLicense = await ResolveLicensesForDependencies(dependencies.ToArray());
            var association = dependenciesWithLicense.Select(l => new DependencyLicenseAssociation
            {
                DependencyIdentifier = $"{l.Item1.DependencyName}@{l.Item1.DependencyVersion}",
                LicenseIdentifier = l.Item2
            });
            var serializedIdentification = JsonConvert.SerializeObject(association);
            await File.WriteAllTextAsync(AssociationFileLocation, serializedIdentification);
            var licenseInformation = dependenciesWithLicense.Select(d => d.Item2)
                .AsParallel()
                .Select(GetLicenseInformation);
            var awaitInformation = await Task.WhenAll(licenseInformation);
            return DistinctByExtension.DistinctBy(awaitInformation.Where(l => l != null), l => l.LicenseId);
        }

        public async Task<IEnumerable<DependencyInformationWithLicenseIdentifier>> GetLicenseAssociation(
            IEnumerable<DependencyInformation> dependencies)
        {
            var deps = dependencies.ToArray();
            if (!File.Exists(AssociationFile))
            {
                await Crawl(deps);
            }

            var content = await File.ReadAllTextAsync(AssociationFileLocation);
            var association = JsonConvert.DeserializeObject<List<DependencyLicenseAssociation>>(content)
                .ToDictionary(k => k.DependencyIdentifier, v => v.LicenseIdentifier);
            return deps
                .Select(d =>
                {
                    var key = $"{d.DependencyName}@{d.DependencyVersion}";
                    return new DependencyInformationWithLicenseIdentifier(d,
                        association.ContainsKey(key) ? association[key] : null);
                });
        }

        public IDictionary<string, LicenseInformation> GetLicenseCache()
        {
            return _cache;
        }
    }
}