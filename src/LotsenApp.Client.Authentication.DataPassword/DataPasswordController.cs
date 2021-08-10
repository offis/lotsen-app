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
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Configuration.User;
using LotsenApp.Client.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LotsenApp.Client.Authentication.DataPassword
{
    [Route("api/authentication/data-password")]
    [ApiController]
    public class DataPasswordController: ControllerBase
    {
        private readonly UserManager<LocalLotsenAppUser> _userManager;
        private readonly IConfigurationStorage _storage;
        private readonly DataPasswordService _service;

        public DataPasswordController(UserManager<LocalLotsenAppUser> userManager, IConfigurationStorage storage, DataPasswordService service)
        {
            _userManager = userManager;
            _storage = storage;
            _service = service;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Verify([FromBody] DataPasswordVerificationDto request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var configuration = await _storage.GetConfigurationForUser(user.Id);
            if (configuration == null)
            {
                return BadRequest();
            }

            if (!OneWayHashFunction.Verify(request.DataPassword, configuration.HashedDataPassword))
            {
                return BadRequest();
            }

            _service.SetDataPassword(user.Id, request.DataPassword);
            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<bool> HasVerifiedDataPassword()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            try
            {
                _service.GetDataPassword(user.Id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }
    }
}