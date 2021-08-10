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
using LotsenApp.Client.Authentication.DataPassword;
using LotsenApp.Client.Configuration;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Cryptography;
using LotsenApp.Client.Http;
using LotsenApp.Client.TanList;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LotsenApp.Client.Authorization
{
    [Route("api/user")]
    [ApiController]
    public class UserController: ControllerBase
    {
        private readonly IConfigurationStorage _configurationStorage;
        private readonly UserManager<LocalLotsenAppUser> _userManager;
        private readonly TanListService _tanListService;
        private readonly DataPasswordService _dataPasswordService;

        public UserController(IConfigurationStorage configurationStorage, UserManager<LocalLotsenAppUser> userManager, TanListService tanListService, DataPasswordService dataPasswordService)
        {
            _configurationStorage = configurationStorage;
            _userManager = userManager;
            _tanListService = tanListService;
            _dataPasswordService = dataPasswordService;
        }


        [HttpGet("me")]
        public async Task<UserDto> GetMe()
        {
            var globalConfiguration = await _configurationStorage.GetGlobalConfiguration();
            if (globalConfiguration.ApplicationMode == ApplicationMode.Offline)
            {
                var localUser = await _userManager.FindByNameAsync(Environment.UserName);
                if (localUser == null)
                {
                    await _userManager.CreateAsync(new LocalLotsenAppUser
                    {
                        Id = OneWayHashFunction.Hash(Environment.UserName),
                        UserName = Environment.UserName
                    });
                }
                return new UserDto
                {
                    Name = Environment.UserName,
                    Roles = new []{"Guide"}
                };
            }
            if (HttpContext.User == null)
            {
                throw new BadRequestException();
            }
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return new UserDto
            {
                Name = user.UserName,
                Roles = (await _userManager.GetRolesAsync(user)).ToArray()
            };
        }

        [HttpGet("sign-out")]
        [Authorize]
        public new async Task<IActionResult> SignOut()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            _dataPasswordService.RemoveDataPassword(user.Id);
            // TODO Remove online session as well
            return Ok(new SignOutDto
            {
                Success = true
            });
        }

        [HttpGet("forbidden")]
        [HttpGet("logout")]
        [HttpPost("forbidden")]
        public IActionResult Forbidden([FromQuery] string returnUrl)
        {
            return StatusCode(403, new ForbiddenDto
            {
                StatusCode = 403,
                RequestUrl = returnUrl,
            });
        }

        [HttpPost("password-recovery")]
        public async Task<IActionResult> RecoverPassword([FromBody] PasswordRecoveryDto recoveryRequest)
        {
            var user = await _userManager.FindByNameAsync(recoveryRequest.UserName);
            if (user == null)
            {
                return StatusCode(404, "The user could not be found");
            }

            if (recoveryRequest.NewPassword != recoveryRequest.NewPasswordRepeat)
            {
                return BadRequest("The passwords do not match");
            }

            var (success, tansLeft) = await _tanListService.UseTan(user.Id, recoveryRequest.Tan);
            if (success)
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var response = await _userManager.ResetPasswordAsync(user, resetToken, recoveryRequest.NewPassword);
                if (response.Succeeded)
                {
                    return Ok(new MessageResponse
                    {
                        Message = tansLeft ? "PasswordRecovery.Success" : "PasswordRecovery.NoMoreTans"
                    });
                }
                
            }

            return StatusCode(422);
        }
    }
}