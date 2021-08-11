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

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using LibGit2Sharp;
using LotsenApp.LicenseManager.Configuration;
using LotsenApp.LicenseManager.DependencyCrawling.Nuget;
using LotsenApp.LicenseManager.LicenseResolving;
using Xunit;

namespace LotsenApp.LicenseManager.Test
{
    [ExcludeFromCodeCoverage]
    [Trait("Type", "Integration")]
    public class LicenseResolverTest
    {
        public static LicenseResolver CreateInstance()
        {
            var httpClient = new HttpClient();
            var configuration = new LicenseManagerConfiguration();
            var npmLicenseResolver = new NpmLicenseResolver();
            var nugetCache = new NugetCache(configuration);
            var nugetService = new NugetService(httpClient, nugetCache);
            var nugetLicenseResolver = new NugetLicenseResolver(nugetService, nugetCache);
            var licenseService = new LicenseService(httpClient);
            return new LicenseResolver(new ILicenseResolver[] {npmLicenseResolver, nugetLicenseResolver}, licenseService, configuration);

        }
        [Fact]
        public async Task TestCrawl()
        {
            try
            {
                var configuration = new LicenseManagerConfiguration();
                var crawlingProcess = CrawlingProcessTest.CreateInstance();
                var dependencies = await crawlingProcess.Crawl(configuration.LotsenAppRepositoryRoot);
                var licenseResolver = CreateInstance();
                var licenses = await licenseResolver.Crawl(dependencies);
                Assert.NotEmpty(licenses);
            }
            catch (NotFoundException)
            {
                // A commit was not found
            }

        }
    }
}