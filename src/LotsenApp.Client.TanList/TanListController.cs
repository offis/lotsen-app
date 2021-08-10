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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LotsenApp.Client.TanList
{
    [Route("api/authorization/tan")]
    [ApiController]
    public class TanListController: ControllerBase
    {
        private readonly TanListService _service;
        private readonly UserManager<LocalLotsenAppUser> _userManager;

        public TanListController(TanListService service, UserManager<LocalLotsenAppUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet("exists")]
        [Authorize]
        public async Task<bool> HasTanList()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _service.HasTanList(user.Id);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GenerateTanList([FromQuery] string tan = null)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (tan == null && await _service.HasTanList(user.Id))
            {
                return BadRequest("A TAN must be provided");
            }
            if (tan == null && !await _service.HasTanList(user.Id))
            {
                // Initial creation of a tan list
                var initialTanList = await _service.CreateOrUpdateTanList(user.Id);
                return CreateResultFromModel(initialTanList);
            }

            var (success, _) = await _service.UseTan(user.Id, tan);
            if (!success)
            {
                return StatusCode(409, "The tan was invalid");
            }

            var tanList = await _service.CreateOrUpdateTanList(user.Id);
            return CreateResultFromModel(tanList);
        }

        private static FileStreamResult CreateResultFromModel(string[] tans)
        {
            const string mimeType = "text/plain";
            var tanFile = string.Join("\n", tans);
            var tanBytes = Encoding.UTF8.GetBytes(tanFile);
            var stream = new MemoryStream(tanBytes.Length);
            stream.Write(tanBytes, 0, tanBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(stream, mimeType)
            {
                FileDownloadName = "tan-list.txt"
            };
        }
    }
}