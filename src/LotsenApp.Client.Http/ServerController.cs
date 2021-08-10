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
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LotsenApp.Client.Http
{
    [Route("api/server")]
    [ApiController]
    public class ServerController: ControllerBase
    {
        private readonly ILogger<ServerController> _logger;
        private readonly IHttpClientProvider _provider;

        public ServerController(ILogger<ServerController> logger, IHttpClientProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestServer([FromBody] ServerTestRequest request)
        {
            try
            {
                using var http = _provider.GetClient();
                http.BaseAddress = new Uri(request.Server);
                var response = await http.GetAsync("api/ping");
                if (response.IsSuccessStatusCode)
                {
                    var stringContent = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<ServerTestResponseDto>(stringContent);
                    return Ok(new ServerTestDto
                    {
                        Success = responseObject?.Name?.StartsWith("LotsenApp.Server.") ?? false
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("There was an error looking for a remote server", ex);
            }


            return BadRequest();
        }
    }

    public class ServerTestRequest
    {
        public string Server { get; set; }
    }

    public class ServerTestResponseDto
    {
        public string Name { get; set; }
    }

    public class ServerTestDto
    {
        public bool Success { get; set; }
    }
}