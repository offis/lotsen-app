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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LotsenApp.LicenseManager.Configuration;
using LotsenApp.LicenseManager.DependencyCrawling.Nuget;
using Xunit;

namespace LotsenApp.LicenseManager.Test
{
    [ExcludeFromCodeCoverage]
    public class NugetServiceTest
    {
        public static  NugetService CreateInstance()
        {
            return new (new HttpClient(), new NugetCache(new LicenseManagerConfiguration()));
        }
        
        [Fact]
        public async Task TestGetCatalog()
        {
            var service = CreateInstance();
            var catalog = await service.GetCatalog();
            Assert.NotNull(catalog);
        }
        
        [Fact]
        public async Task TestGetMetadataService()
        {
            var service = CreateInstance();
            var catalog = await service.GetCatalog();
            var resource = service.GetMetadataServiceUrl(catalog);
            Assert.NotNull(resource);
        }
        
        [Fact]
        public async Task TestGetRegistrationEntry()
        {
            var service = CreateInstance();
            var catalog = await service.GetCatalog();
            var resource = service.GetMetadataServiceUrl(catalog);
            var entry = await service.GetRegistrationEntry(resource, "Newtonsoft.Json", "12.0.3");
            Assert.NotNull(entry);
        }
        
        [Fact]
        public async Task TestGetCatalogEntry()
        {
            var service = CreateInstance();
            var catalog = await service.GetCatalog();
            var resource = service.GetMetadataServiceUrl(catalog);
            var entry = await service.GetRegistrationEntry(resource, "morelinq", "3.3.2");
            var catalogEntry = await service.GetCatalogEntry(entry);
            Assert.NotNull(catalogEntry);
        }
        
        [Fact]
        public async Task TestGetSpdxIdentifier()
        {
            var service = CreateInstance();
            var catalog = await service.GetCatalog();
            var resource = service.GetMetadataServiceUrl(catalog);
            var entry = await service.GetRegistrationEntry(resource, "Microsoft.EntityFrameworkCore", "5.0.3");
            var catalogEntry = await service.GetCatalogEntry(entry);
            var spdxIdentifier = service.GetSpdxIdentifier(catalogEntry);
            Assert.NotNull(spdxIdentifier);
        }
        
        [Fact]
        public async Task TestGetDependencyInformation()
        {
            var service = CreateInstance();
            var catalog = await service.GetCatalog();
            var resource = service.GetMetadataServiceUrl(catalog);
            var entry = await service.GetRegistrationEntry(resource, "Microsoft.EntityFrameworkCore", "5.0.3");
            var catalogEntry = await service.GetCatalogEntry(entry);
            var dependencyInformation = await service
                .GetDependencyInformation(catalogEntry.DependencyGroups.First().Dependencies.First(), resource);
            Assert.NotNull(dependencyInformation);
        }
    }
}