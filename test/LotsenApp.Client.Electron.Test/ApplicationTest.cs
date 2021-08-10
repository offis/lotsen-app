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
using System.Net;
using System.Threading.Tasks;
using Ical.Net.DataTypes;
using LotsenApp.Client.Authentication.Api;
using LotsenApp.Client.Electron.Controllers;
using Newtonsoft.Json;
using Xunit;

namespace LotsenApp.Client.Electron.Test
{
    [ExcludeFromCodeCoverage]
    [Trait("Type", "Integration")]
    public class ApplicationTest: IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public ApplicationTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ShouldStart()
        {
            using var client = _factory.CreateClient();

            var response = await client.GetStringAsync("api/cryptography/transient");
            var dto = JsonConvert.DeserializeObject<PublicKeyDto>(response);
            Assert.StartsWith("-----BEGIN RSA PUBLIC KEY-----", dto?.PublicKey!);
        }
        
        [Fact]
        public async Task ShouldReachOfflineAuthenticationPlugin()
        {
            using var client = _factory.CreateClient();

            var response = await client.GetAsync("api/authentication/offline");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<UserDto>(content);
                Assert.Equal(Environment.UserName, dto?.Name);
            }
            else
            {
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            }
        }
    }
}