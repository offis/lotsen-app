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
using LotsenApp.LicenseManager.Configuration;
using LotsenApp.LicenseManager.DependencyCrawling;
using LotsenApp.LicenseManager.LicenseResolving;
using Newtonsoft.Json;

namespace LotsenApp.LicenseManager.LicenseCreation
{
    public class LicenseCreator
    {
        private readonly LicenseResolver _resolver;
        private readonly CrawlingProcess _process;
        private readonly LicenseManagerConfiguration _configuration;

        public LicenseCreator(LicenseResolver resolver, CrawlingProcess process, LicenseManagerConfiguration configuration)
        {
            _resolver = resolver;
            _process = process;
            _configuration = configuration;
        }
        public async Task<string> CreateThirdPartyLicenseFile()
        {
            var dependencies = await _process.Crawl();
            var licenseAssociation = await _resolver.GetLicenseAssociation(dependencies);
            var licenses = _resolver.GetLicenseCache();

            var orderedDependencies
                = licenseAssociation.Where(l => l.LicenseIdentifier != "no-license")
                .OrderBy(d => d.DirectDependency ? 0 : 1);
            var licenseTexts = orderedDependencies.Select(d =>
                $"{d.DependencyName} {d.DependencyVersion}\n\n {licenses[d.LicenseIdentifier].LicenseText}")
                .Aggregate("The order of the dependencies in this file is only related to the fact that the direct " +
                           "dependencies of this repository are listed first and the transitive dependencies are listed " +
                           "below. Otherwise, the order does not imply importance or other factors for this project.\n\n", 
                    (cur, next) => cur + "\n\n" + next);
            
            var thirdPartyLicenseFile = Path.Join(_configuration.LotsenAppRepositoryRoot, "THIRD_PARTY_LICENSE.txt");
            await File.WriteAllTextAsync(thirdPartyLicenseFile, licenseTexts);
            return licenseTexts;
        }

        public async Task<string> CreateLicenseFile()
        {
            var licenseFileStream = GetType().Assembly
                .GetManifestResourceStream("LotsenApp.LicenseManager.Assets.License.json");
            if (licenseFileStream == null)
            {
                throw new Exception("The license resource could not be found");
            }
            using var streamReader = new StreamReader(licenseFileStream);
            var content = await streamReader.ReadToEndAsync();
            var license = JsonConvert.DeserializeObject<ProjectLicense>(content);
            var licenseText = license.LicenseText
                .Replace("{{year}}", DateTime.Now.Year + "")
                .Replace("{{company}}", "OFFIS e.V.");
            var licenseFile = Path.Join(_configuration.LotsenAppRepositoryRoot, "LICENSE.txt");
            
            await File.WriteAllTextAsync(licenseFile, licenseText);
            return licenseText;
        }
    }
}