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
using LotsenApp.Client.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LotsenApp.Client.Authentication.Password
{
    [Route("api/authentication/password")]
    [ApiController]
    public class PasswordAuthenticationController: ControllerBase
    {
        private readonly IConfigurationStorage _configurationStorage;
        private readonly PasswordAuthenticator _authenticator;
        private readonly UserManager<LocalLotsenAppUser> _userManager;
        private readonly IOptions<IdentityOptions> _options;

        public PasswordAuthenticationController(IConfigurationStorage configurationStorage, 
            PasswordAuthenticator authenticator, 
            UserManager<LocalLotsenAppUser> userManager, 
            IOptions<IdentityOptions> options)
        {
            _configurationStorage = configurationStorage;
            _authenticator = authenticator;
            _userManager = userManager;
            _options = options;
        }

        [HttpGet]
        [Authorize]
        public async Task<bool> HasPassword()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _userManager.HasPasswordAsync(user);
        }

        [HttpPut]
        [Authorize]
        public async Task SetPassword([FromBody] SetPasswordDto request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _userManager.AddPasswordAsync(user, request.Password);
        }

        [HttpPost]
        public async Task<IActionResult> PasswordAuthentication([FromBody] PasswordAuthenticationRequest request)
        {
            try
            {
                var localUser = await _userManager.GetUserAsync(HttpContext.User);
                var server = request.Server.EndsWith("/") ? request.Server : $"{request.Server}/";
                var (userId, onlineAuthenticated) = await _authenticator.Authenticate(request, server, localUser?.Id);
                var user = _authenticator.User;
                var identityUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.OnlineId == userId || u.Id == userId);
                if (identityUser == null && localUser == null)
                {
                    // var id = Guid.NewGuid().ToString();
                    var newUser = new LocalLotsenAppUser
                    {
                        Id = userId,
                        UserName = user.Name,
                        NormalizedUserName = user.Name.ToUpper(),
                        OnlineId = onlineAuthenticated ? userId : null,
                    };
                    var result = await _userManager.CreateAsync(newUser);
                    var userConfiguration = await _configurationStorage.GetConfigurationForUser(newUser.Id, AccessMode.Write);
                    userConfiguration.SynchronisationConfiguration.Server = userConfiguration.SynchronisationConfiguration.Server.Append(server).ToArray();
                    await _configurationStorage.SaveUserConfiguration(userConfiguration);
                    if (!result.Succeeded)
                    {
                        return BadRequest("The user does no exist and cannot be created");
                    }

                    identityUser = await _userManager.Users
                        .FirstAsync(u => u.OnlineId == userId || u.Id == userId);
                }
                else if (localUser != null && localUser.OsUserAccount)
                {
                    localUser.OnlineId = user.UserId;
                    localUser.UserName = user.Name;
                    var userConfiguration = await _configurationStorage.GetConfigurationForUser(localUser.Id, AccessMode.Write);
                    userConfiguration.SynchronisationConfiguration.Server = userConfiguration.SynchronisationConfiguration.Server.Append(server).ToArray();
                    await _configurationStorage.SaveUserConfiguration(userConfiguration);
                    await _userManager.UpdateAsync(localUser);
                    identityUser = localUser;
                    var globalConfiguration = await _configurationStorage.GetGlobalConfiguration(AccessMode.Write);
                    globalConfiguration.ApplicationMode = ApplicationMode.Online;
                    await _configurationStorage.SaveGlobalConfiguration(globalConfiguration);
                }
                else if (identityUser == null) 
                {
                    return BadRequest("You are already authenticated");
                }


                await _userManager.AddToRolesAsync(identityUser, _authenticator.User.Roles);
                var principal = new UserClaimsPrincipalFactory<LocalLotsenAppUser>(_userManager, _options);
                if (localUser == null)
                {
                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme,
                        await principal.CreateAsync(identityUser), new AuthenticationProperties
                        {
                            IsPersistent = request.RememberMe,
                            AllowRefresh = request.RememberMe,
                        });
                }
                return Ok(new UserDto
                {
                    Id = identityUser.Id,
                    Name = identityUser.UserName,
                    Roles = (await _userManager.GetRolesAsync(identityUser)).ToArray(),
                    IsOnline = identityUser.IsOnline
                });
            }
            catch (ServerNotFoundException)
            {
                return NotFound("The server does not support this authentication.");
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InternalServerException ex)
            {
                return StatusCode(500, ex.Message);
            }
            catch (ServerUnreachableException ex)
            {
                return StatusCode(503, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }
    }
}