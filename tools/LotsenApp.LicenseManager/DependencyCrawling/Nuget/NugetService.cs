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
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LotsenApp.LicenseManager.DependencyCrawling.Nuget
{
    public class NugetService
    {
        private readonly HttpClient _httpClient;
        private readonly NugetCache _cache;

        private NugetResource metadataService;

        public NugetService(HttpClient httpClient, NugetCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<NugetCatalog> GetCatalog()
        {
            const string serviceUrl = "https://api.nuget.org/v3/index.json";
            var response = await _httpClient.GetAsync(serviceUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<NugetCatalog>(content);
            }

            throw new Exception("The service could not be determined");
        }

        public NugetResource GetMetadataServiceUrl(NugetCatalog catalog)
        {
            const string identifier = "RegistrationsBaseUrl/3.6.0";
            var resource = catalog.Resources.First(r => r.Type == identifier);
            return resource;
        }

        public async Task<NugetResource> GetMetadataServiceUrl()
        {
            if (metadataService != null)
            {
                return metadataService;
            }

            var catalog = await GetCatalog();
            return metadataService = GetMetadataServiceUrl(catalog);
        }

        public async Task<NugetRegistrationEntry> GetRegistrationEntry(NugetResource resource, string packageName, string version)
        {
            var requestUrl = $"{resource.Id}{packageName.ToLower()}/{version}.json";
            var response = await _httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStreamAsync();
                using var decompressionStream = new GZipStream(content, CompressionMode.Decompress);
                using var streamReader = new StreamReader(decompressionStream);
                var deflatedContent = await streamReader.ReadToEndAsync();
                return JsonConvert.DeserializeObject<NugetRegistrationEntry>(deflatedContent);
            }

            throw new Exception($"The entry {packageName}@{version} could not be found");
        }

        public async Task<NugetCatalogEntry> GetCatalogEntry(NugetRegistrationEntry registrationEntry)
        {
            var response = await _httpClient.GetAsync(registrationEntry.CatalogEntry);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<NugetCatalogEntry>(content);
            }

            throw new Exception("The catalog entry could not be found");
        }

        public async Task<NugetCatalogEntry> GetDependencyInformation(Dependency dependency, NugetResource service = null)
        {
            var versionRangeRegex = new Regex(@"\[?(\d+\.\d+\.\d+)");
            var packageName = dependency.Id;
            var match = versionRangeRegex.Match(dependency.Range);
            var version = match.Groups[1].Value;
            if (_cache.TryGetCached(packageName, version, out var cachedEntry))
            {
                return cachedEntry;
            }
            NugetRegistrationEntry registrationEntry;
            if(service != null)
            {
                registrationEntry = await GetRegistrationEntry(service, packageName, version);
            }
            else
            {
                metadataService ??= await GetMetadataServiceUrl();
                registrationEntry = await GetRegistrationEntry(metadataService, packageName, version);
            }

            var catalogEntry = await GetCatalogEntry(registrationEntry);
            _cache.Cache(packageName, version, catalogEntry);
            return catalogEntry;
        }

        public string GetSpdxIdentifier(NugetCatalogEntry entry)
        {
            return entry.LicenseExpression ?? entry.LicenseUrl;
        }
    }
}