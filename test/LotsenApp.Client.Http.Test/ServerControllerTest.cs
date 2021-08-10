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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LotsenApp.Client.Http.Test
{
    [ExcludeFromCodeCoverage]
    public class ServerControllerTest: IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private readonly Mock<ILogger<ServerController>> _loggerMock;
        private readonly IHttpClientProvider _provider;

        public ServerControllerTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _provider = new TestHttpClientProvider(_factory);
            _loggerMock = new Mock<ILogger<ServerController>>();
        }
        
        [Fact]
        public async Task ShouldTestAvailabilityOnExistingServer()
        {
            var controller = new ServerController(_loggerMock.Object, _provider);
            var url = _factory.Server.BaseAddress.ToString();

            var response = await controller.TestServer(new ServerTestRequest
            {
                Server = url
            });
            Assert.IsType<OkObjectResult>(response);
            var castResponse = response as OkObjectResult;
            Assert.IsType<ServerTestDto>(castResponse?.Value);
            var value = castResponse.Value as ServerTestDto;
            Assert.True(value?.Success);
        }
        
        [Fact]
        public async Task ShouldReturnBadRequestOnWrongServer()
        {
            var controller = new ServerController(_loggerMock.Object, new StandardHttpClientProvider());
            var response = await controller.TestServer(new ServerTestRequest
            {
                Server = "http://localhost:28001/"
            });
            Assert.IsType<BadRequestResult>(response);
        }
    }
}