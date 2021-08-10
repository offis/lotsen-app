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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LotsenApp.LicenseManager.Configuration;
using LotsenApp.LicenseManager.DependencyCrawling;
using LotsenApp.LicenseManager.DependencyCrawling.Npm;
using LotsenApp.LicenseManager.DependencyCrawling.Nuget;
using MoreLinq;
using Xunit;

namespace LotsenApp.LicenseManager.Test
{
    [ExcludeFromCodeCoverage]
    public class CrawlingProcessTest
    {
        public static CrawlingProcess CreateInstance()
        {
            var configuration = new LicenseManagerConfiguration();
            var httpClient = new HttpClient();
            var cache = new NugetCache(configuration);
            var npmCrawler = new NpmCrawler();
            var nugetService = new NugetService(httpClient, cache);
            var csprojCrawler = new CsProjCrawler(nugetService, cache);
            return new CrawlingProcess(
                new ProjectFileCrawler(new IProjectCrawler[] {npmCrawler, csprojCrawler}), configuration);
        }
        // [Fact]
        public async Task TestProcess()
        {
            var process = CreateInstance();

            var directory = Environment.CurrentDirectory;
            var initialDirectory = Path.Join(directory, "../../../../../"); // "../../../../../" 
            var dependencies = (await process.Crawl(initialDirectory)).ToArray();
            var distinctDependencies = dependencies.DistinctBy(d => (d.DependencyName, d.DependencyVersion)).ToArray();
        }
        
        // [Fact]
        public async Task TestProcessInformation()
        {
            var process = CreateInstance();

            var directory = Environment.CurrentDirectory;
            var initialDirectory = Path.Join(directory, "../../../../../");
            var dependencies = (await process.Crawl(initialDirectory)).ToArray();
            //var licenses = await process.Crawl(dependencies);
        }
    }
}