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
using System.Linq;
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using LotsenApp.Client.Configuration;
using LotsenApp.Client.Configuration.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LotsenApp.Client.Authentication.Offline
{
    [Route("api/authentication/offline")]
    [ApiController]
    public class OfflineAuthenticationController: ControllerBase
    {
        private readonly IConfigurationStorage _configurationStorage;
        private readonly UserManager<LocalLotsenAppUser> _userManager;
        private readonly UserContext _userContext;
        private readonly IOptions<IdentityOptions> _options;
        private readonly RoleManager<IdentityRole> _roleManager;

        public OfflineAuthenticationController(IConfigurationStorage configurationStorage, 
            UserManager<LocalLotsenAppUser> userManager, 
            UserContext userContext,
            IOptions<IdentityOptions> options,
            RoleManager<IdentityRole> roleManager)
        {
            _configurationStorage = configurationStorage;
            _userManager = userManager;
            _userContext = userContext;
            _options = options;
            _roleManager = roleManager;
        }

        [HttpGet("password")]
        public async Task<IActionResult> OfflineUserHasPassword()
        {
            var user = await _userContext.Users.FirstOrDefaultAsync(u => u.OsUserAccount);
            return Ok(new OfflineUserHasPasswordDto
            {
                HasPassword = user != null && await _userManager.HasPasswordAsync(user)
            });
        }
        
        [HttpGet]
        public async Task<IActionResult> UseOfflineUser()
        {
            var globalConfiguration = await _configurationStorage.GetGlobalConfiguration();
            if (globalConfiguration.ApplicationMode != ApplicationMode.Offline)
                return BadRequest(new BadRequestDto
                {
                    Message = "The offline authentication is not supported in online mode"
                });

            var user = await _userContext.Users.FirstOrDefaultAsync(u => u.OsUserAccount);
            user = await CreateIfNotExists(user);
            if (await _userManager.HasPasswordAsync(user))
            {
                return Conflict();
            }

            var principal = new UserClaimsPrincipalFactory<LocalLotsenAppUser>(_userManager, _options);
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme,
                await principal.CreateAsync(user), new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                });

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray(),
                IsOnline = false
            });

        }


        private async Task<LocalLotsenAppUser> CreateIfNotExists(LocalLotsenAppUser user)
        {
            if (user != null)
            {
                return user;
            }

            await _userManager.CreateAsync(new LocalLotsenAppUser
            {
                UserName = Environment.UserName,
                OsUserAccount = true,
            });
            user = await _userManager.FindByNameAsync(Environment.UserName);
            await _roleManager.CreateAsync(new IdentityRole("Guide"));
            await _userManager.AddToRolesAsync(user, new[] {"Guide"});
            return user;
        }
    }

    class BadRequestDto
    {
        public string Message { get; set; }
    }
}
