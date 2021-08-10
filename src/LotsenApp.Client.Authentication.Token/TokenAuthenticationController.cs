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

using System.Linq;
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using LotsenApp.Client.Authorization;
using LotsenApp.Client.Configuration;
using LotsenApp.Client.Configuration.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LotsenApp.Client.Authentication.Token
{
    [Route("api/authentication/token")]
    [ApiController]
    public class TokenAuthenticationController: ControllerBase
    {
        private readonly TokenAuthenticator _authenticator;
        private readonly TokenStorage _storage;
        private readonly UserManager<LocalLotsenAppUser> _userManager;

        public TokenAuthenticationController(TokenAuthenticator authenticator, TokenStorage storage, UserManager<LocalLotsenAppUser> userManager, IOptions<IdentityOptions> options)
        {
            _authenticator = authenticator;
            _storage = storage;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> TokenAuthentication()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return StatusCode(403);
            }

            var refreshToken = _storage.GetRefreshToken(user.Id);
            if (refreshToken != null)
            {
                var newToken = await _authenticator.Authenticate(new TokenAuthenticationRequest
                {
                    RefreshToken = refreshToken
                }, user);
                if (newToken != refreshToken)
                {
                    _storage.SetRefreshToken(user.Id, newToken);
                }
            }
            
            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray(),
                IsOnline = user.IsOnline
            });
        }
    }
}