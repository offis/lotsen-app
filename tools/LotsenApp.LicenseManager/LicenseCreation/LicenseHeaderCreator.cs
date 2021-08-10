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
using LotsenApp.LicenseManager.Configuration;
using LotsenApp.LicenseManager.LicenseResolving;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LotsenApp.LicenseManager.LicenseCreation
{
    public class LicenseHeaderCreator
    {
        private static readonly string[] IgnoredFolders = {"node_modules", "obj", "bin", ".idea", "dist", "keys"};
        private readonly ILicenseHeaderFormatter[] _formatters;
        private readonly LicenseHeaderFormatter _formatter;
        private readonly LicenseManagerConfiguration _configuration;
        private readonly ILogger<LicenseHeaderCreator> _logger;
        private readonly LicenseModel _licenseModel;

        public LicenseHeaderCreator(IEnumerable<ILicenseHeaderFormatter> formatters, LicenseHeaderFormatter formatter, LicenseManagerConfiguration configuration, ILogger<LicenseHeaderCreator> logger)
        {
            _formatters = formatters.ToArray();
            _formatter = formatter;
            _configuration = configuration;
            _logger = logger;

            var licenseStream = GetType().Assembly.GetManifestResourceStream("LotsenApp.LicenseManager.Assets.License.json");
            if (licenseStream == null)
            {
                throw new Exception("Could not read license information");
            }
            using var streamReader = new StreamReader(licenseStream);
            var licenseContent = streamReader.ReadToEnd();
            _licenseModel = JsonConvert.DeserializeObject<LicenseModel>(licenseContent);
        }

        public void CrawlFiles(string initialDirectory)
        {
            
            if (initialDirectory == null || IgnoredFolders.Any(initialDirectory.Contains))
            {
                return;
            }
            var directory = new DirectoryInfo(initialDirectory);
            foreach (var fileInfo in directory.GetFiles())
            {
                CheckHeader(fileInfo.FullName);
            }
            foreach (var directoryInfo in directory.GetDirectories())
            {
                CrawlFiles(directoryInfo.FullName);
            }
            
            
        }

        public void CheckHeader(string file)
        {
            if (file == null)
            {
                return;
            }

            var formatter = _formatters.FirstOrDefault(f => f.SupportedFileExtension.Any(file.EndsWith));
            // file type is unknown and will be skipped.
            if (formatter == null)
            {
                return;
            }

            var licenseText = _licenseModel.LicenseText;
            var replacements = new Dictionary<string, string>
            {
                {_licenseModel.YearReplacement, DateTime.Now.Year + ""},
                {_licenseModel.AuthorReplacement, "OFFIS e.V."}
            };
            _formatter.SetOrUpdateHeader(file, licenseText, replacements, formatter);

        }

        public void CrawlFiles()
        {
            var initialDirectory = _configuration.LotsenAppRepositoryRoot;
            _logger.LogInformation("Crawling in directory " + initialDirectory);
            CrawlFiles(initialDirectory);
            _logger.LogInformation("Finished crawling in directory " + initialDirectory);
        }
    }
}