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
using System.Threading.Tasks;
using LotsenApp.LicenseManager.DependencyCrawling;
using Newtonsoft.Json.Linq;

namespace LotsenApp.LicenseManager.LicenseResolving
{
    public class NpmLicenseResolver: ILicenseResolver
    {
        
        public DependencyType SupportedDependencyType => DependencyType.Npm;
        
        public Task<string> GetSpdxIdentifier(DependencyInformation dependencyInformation)
        {
            var dependencyModule = Path.Join(dependencyInformation.Origin, "package.json"); // "node_modules", dependencyInformation.DependencyName
            var content = File.ReadAllText(dependencyModule);
            var packageModel = JObject.Parse(content);
            try
            {
                var license = packageModel["license"];
                if (license is JObject licenseObject)
                {
                    var url = licenseObject["url"]?.Value<string>();
                    var type = licenseObject["type"]?.Value<string>();
                    return Task.FromResult(url ?? type);
                }
                var spdxIdentifier = license?.Value<string>();
                return Task.FromResult(spdxIdentifier);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"A license could not be resolved for {dependencyInformation.DependencyName} in {dependencyInformation.Origin}",
                    ex);
            }
        }
    }
}