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
using System.Threading;
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace LotsenApp.Client.Configuration.Rest
{
    [Route("api/configuration/user")]
    [ApiController]
    public class UserConfigurationController: ControllerBase
    {
        private readonly UserManager<LocalLotsenAppUser> _userManager;
        private readonly IUserConfigurationRestService _restService;

        public UserConfigurationController(UserManager<LocalLotsenAppUser> userManager, IUserConfigurationRestService restService)
        {
            _userManager = userManager;
            _restService = restService;
        }

        [HttpGet]
        [Authorize]
        public async Task<UserConfigurationDto> GetUserConfiguration()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            
            return await _restService.GetUserConfiguration(user.Id);
        }
        
        [HttpPost]
        [Authorize]
        public async Task SetUserConfiguration([FromBody] UserConfigurationDto dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            await _restService.SetUserConfiguration(user.Id, dto);
        }

        [HttpGet("app-theme")]
        public async Task<ApplicationThemeDto> GetApplicationTheme()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _restService.GetApplicationTheme(user?.Id);
        }

        [HttpPost("app-theme")]
        [Authorize]
        public async Task SetApplicationTheme([FromBody] ApplicationThemeDto dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _restService.SetApplicationTheme(user.Id, dto);
        }

        [HttpGet("language")]
        public async Task<LanguageDto> GetLanguage()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _restService.GetLanguage(user?.Id);
        }

        [HttpPost("language")]
        [Authorize]
        public async Task SetLanguage([FromBody] LanguageDto dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _restService.SetLanguage(user.Id, dto);
        }

        [HttpGet("first-time")]
        [Authorize]
        public async Task<bool> IsFirstTime()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _restService.IsFirstTime(user.Id);
        }

        [HttpGet("first-time-complete")]
        [Authorize]
        public async Task FirstTimeCompleted()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _restService.FirstTimeCompleted(user.Id);
        }


        [HttpPost("data-password")]
        [Authorize]
        public async Task<IActionResult> SaveDataPassword([FromBody] DataPasswordDto request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            try
            {
                await _restService.SaveDataPassword(user.Id, request);
                return Ok();
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPut("data-password")]
        [Authorize]
        public async Task<IActionResult> ReplaceDataPassword([FromBody] ReplaceDataPasswordDto request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            try
            {
                await _restService.ReplaceDataPassword(user.Id, request);
                return Ok();
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("recovery-key")]
        [Authorize]
        public async Task<IActionResult> ReplaceRecoveryKey([FromBody] ReplaceDataPasswordDto request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            try
            {
                await _restService.ReplaceRecoveryKey(user.Id, request);
                return Ok();
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("data-password")]
        [Authorize]
        public async Task<bool> HasDataPassword()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _restService.HasDataPassword(user.Id);
        }
        
        [HttpGet("dashboard")]
        [Authorize]
        public async Task<DashboardConfiguration[]> LoadDashboardConfiguration()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _restService.LoadDashboardConfiguration(user.Id);
        }
        
        [HttpPost("dashboard")]
        [Authorize]
        public async Task UpdateDashboardConfiguration([FromBody] DashboardConfiguration[] newConfiguration)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _restService.UpdateDashboardConfiguration(user.Id, newConfiguration);
        }
        
        [HttpGet("programme")]
        [Authorize]
        public async Task<ProgrammeDefinition[]> LoadProgrammeConfiguration()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _restService.LoadProgrammeConfiguration(user.Id);
        }
        
        [HttpPost("programme")]
        [Authorize]
        public async Task UpdateProgrammeConfiguration([FromBody] ProgrammeDefinition[] newConfiguration)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _restService.UpdateProgrammeConfiguration(user.Id, newConfiguration);
        }
        
        [HttpGet("reminder")]
        [Authorize]
        public async Task<ReminderConfiguration> LoadReminderConfiguration()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _restService.LoadReminderConfiguration(user.Id);
        }
        
        [HttpPost("reminder")]
        [Authorize]
        public async Task UpdateReminderConfiguration([FromBody] ReminderConfiguration newConfiguration)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _restService.UpdateReminderConfiguration(user.Id, newConfiguration);
        }
    }
}