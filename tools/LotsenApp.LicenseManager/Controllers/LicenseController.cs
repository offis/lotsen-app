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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LotsenApp.LicenseManager.Configuration;
using LotsenApp.LicenseManager.DependencyCrawling;
using LotsenApp.LicenseManager.LicenseCreation;
using LotsenApp.LicenseManager.LicenseResolving;
using Microsoft.AspNetCore.Mvc;
using MoreLinq;
using Newtonsoft.Json;

namespace LotsenApp.LicenseManager.Controllers
{
    [ApiController]
    [Route("/license")]
    public class LicenseController
    {
        private readonly LicenseResolver _resolver;
        private readonly CrawlingProcess _crawlingProcess;
        private readonly LicenseHeaderCreator _headerCreator;
        private readonly LicenseManagerConfiguration _configuration;
        private readonly LicenseCreator _licenseCreator;

        public LicenseController(LicenseResolver resolver,
            CrawlingProcess crawlingProcess,
            LicenseHeaderCreator headerCreator,
            LicenseManagerConfiguration configuration,
            LicenseCreator licenseCreator)
        {
            _resolver = resolver;
            _crawlingProcess = crawlingProcess;
            _headerCreator = headerCreator;
            _configuration = configuration;
            _licenseCreator = licenseCreator;
        }

        [HttpGet]
        public async Task<LicenseModel> GetLicense()
        {
            var resourceStream = GetType().Assembly
                .GetManifestResourceStream("LotsenApp.LicenseManager.Assets.License.json");
            if (resourceStream == null)
            {
                throw new Exception("The License Resource does not exist");
            }

            using var streamReader = new StreamReader(resourceStream);
            var license = await streamReader.ReadToEndAsync();
            var model = JsonConvert.DeserializeObject<LicenseModel>(license);
            return model;
        }

        [HttpPost]
        public void AddLicense([FromBody] LicenseInformation model)
        {
            _resolver.UpdateLicenseCache(model);
        }

        [HttpGet("crawl")]
        public async Task<IEnumerable<DependencyInformation>> CrawlForDependencies()
        {
            return await _crawlingProcess.Crawl();
        }

        [HttpGet("association")]
        public async Task<IEnumerable<DependencyInformationWithLicenseIdentifier>> GetLicenseAssociation()
        {
            var dependencies = await _crawlingProcess.Crawl();
            return await _resolver.GetLicenseAssociation(dependencies);
        }

        [HttpGet("licenses")]
        public async Task<IEnumerable<LicenseInformation>> GetLicenses()
        {
            var dependencies = await _crawlingProcess.Crawl();
            return await _resolver.Crawl(dependencies);
        }

        [HttpGet("header")]
        public void SetHeader()
        {
            _headerCreator.CrawlFiles();
        }

        [HttpGet("the-thing")]
        public async Task DoTheThing()
        {
            // Create the license file
            var licenseText = await _licenseCreator.CreateLicenseFile();
            // Copy the license file into the ClientApp project
            var clientAppLicenseFile =
                Path.Join(_configuration.LotsenAppRepositoryRoot,
                    "src/LotsenApp.Client.Electron/ClientApp/src/assets/legal/LICENSE.txt");
            await File.WriteAllTextAsync(clientAppLicenseFile, licenseText);
            // Create the third party license file
            var thirdPartyLicenseText = await _licenseCreator.CreateThirdPartyLicenseFile();
            // Copy the third-party license file into the ClientApp project
            var thirdPartyLicenseFile =
                Path.Join(_configuration.LotsenAppRepositoryRoot,
                    "src/LotsenApp.Client.Electron/ClientApp/src/assets/legal/THIRD_PARTY_LICENSE.txt");
            await File.WriteAllTextAsync(thirdPartyLicenseFile, thirdPartyLicenseText);
            // Create direct dependencies file in client app project.
            var dependencies = await GetLicenseAssociation();
            var directDependencies = dependencies
                .Where(d => d.DirectDependency)
                .DistinctBy(d => (d.DependencyName, d.DependencyVersion));
            var serializedInformation = JsonConvert.SerializeObject(directDependencies);
            var directDependencyFile = Path.Join(_configuration.LotsenAppRepositoryRoot,
                "src/LotsenApp.Client.Electron/ClientApp/src/assets/legal/direct_dependencies.json"
            );
            await File.WriteAllTextAsync(directDependencyFile, serializedInformation);
            // Create license header
            SetHeader();
        }
    }
}