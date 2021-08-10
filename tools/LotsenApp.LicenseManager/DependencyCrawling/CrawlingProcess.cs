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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using LotsenApp.LicenseManager.Configuration;
using Newtonsoft.Json;

namespace LotsenApp.LicenseManager.DependencyCrawling
{
    public class CrawlingProcess
    {
        private readonly ProjectFileCrawler _fileCrawler;
        private readonly LicenseManagerConfiguration _configuration;

        public CrawlingProcess(ProjectFileCrawler fileCrawler, LicenseManagerConfiguration configuration)
        {
            _fileCrawler = fileCrawler;
            _configuration = configuration;
        }

        public async Task<IEnumerable<DependencyInformation>> Crawl()
        {
            return await Crawl(_configuration.LotsenAppRepositoryRoot);
        }

        public async Task<IEnumerable<DependencyInformation>> Crawl(string initialDirectory)
        {
            var cache = await CheckForExistingCrawling(_configuration.LotsenAppRepositoryRoot, _configuration.CacheFolder);

            if (cache != null)
            {
                return cache;
            }
            
            var dependencies = (await _fileCrawler.Crawl(initialDirectory)).ToArray();
            // var dependenciesWithLicense = await _licenseManager.ResolveLicensesForDependencies(dependencies.ToArray());
            // var licenseInformation = dependenciesWithLicense.Select(d => d.Item2)
            //     .Select(l => _licenseManager.GetLicenseInformation(l));
            // var awaitInformation = await Task.WhenAll(licenseInformation);
            await WriteCache(dependencies, _configuration.LotsenAppRepositoryRoot, _configuration.CacheFolder);
            return dependencies;
        }

        public async Task<IEnumerable<DependencyInformation>> CheckForExistingCrawling(string repositoryRoot, string cacheFolder)
        {
            using var repository = new Repository(repositoryRoot);
            var latestCommit = repository.Commits.First();
            var latestCommitId = latestCommit.Id.Sha;
            var dependencyFileName = Path.Join(cacheFolder, latestCommitId + ".json");
            if (!File.Exists(dependencyFileName))
            {
                return null;
            }
            var content = await File.ReadAllTextAsync(dependencyFileName);
            return JsonConvert.DeserializeObject<IEnumerable<DependencyInformation>>(content);
        }

        public async Task WriteCache(IEnumerable<DependencyInformation> dependencyInformation, string repositoryRoot, string cacheDirectory)
        {
            using var repository = new Repository(repositoryRoot);
            var latestCommit = repository.Commits.First();
            var latestCommitId = latestCommit.Id.Sha;
            var dependencyFileName = Path.Join(cacheDirectory, latestCommitId + ".json");
            var serializedContent = JsonConvert.SerializeObject(dependencyInformation);
            await File.WriteAllTextAsync(dependencyFileName, serializedContent);
        }
        
        
    }
}