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

using System.Threading.Tasks;
using LotsenApp.LicenseManager.DependencyCrawling;
using LotsenApp.LicenseManager.DependencyCrawling.Nuget;

namespace LotsenApp.LicenseManager.LicenseResolving
{
    public class NugetLicenseResolver: ILicenseResolver
    {
        private readonly NugetService _nugetService;
        private readonly NugetCache _nugetCache;
        public DependencyType SupportedDependencyType => DependencyType.Nuget;
        
        public NugetLicenseResolver(NugetService nugetService, NugetCache nugetCache)
        {
            _nugetService = nugetService;
            _nugetCache = nugetCache;
        }

        public async Task<string> GetSpdxIdentifier(DependencyInformation information)
        {
            if (_nugetCache.TryGetCached(information.DependencyName, information.DependencyVersion,
                out var cachedEntry))
            {
                return _nugetService.GetSpdxIdentifier(cachedEntry);
            }
            var service = await _nugetService.GetMetadataServiceUrl();

            var registration = await _nugetService.GetRegistrationEntry(service, information.DependencyName, information.DependencyVersion);
            var entry = await _nugetService.GetCatalogEntry(registration);
            _nugetCache.Cache(information.DependencyName, information.DependencyVersion, entry);
            return _nugetService.GetSpdxIdentifier(entry);
        }
    }
}