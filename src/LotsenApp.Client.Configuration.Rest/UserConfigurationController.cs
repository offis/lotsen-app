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
        private readonly IConfigurationStorage _storage;
        private readonly UserConfigurationService _configurationService;

        public UserConfigurationController(UserManager<LocalLotsenAppUser> userManager, IConfigurationStorage storage, UserConfigurationService configurationService)
        {
            _userManager = userManager;
            _storage = storage;
            _configurationService = configurationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<UserConfigurationDto> GetUserConfiguration()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var userConfiguration = await _storage.GetConfigurationForUser(user.Id);

            return new UserConfigurationDto(userConfiguration);
        }
        
        [HttpPost]
        [Authorize]
        public async Task SetUserConfiguration([FromBody] UserConfigurationDto dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var userConfiguration = await _storage.GetConfigurationForUser(user.Id, AccessMode.Write);

            await _storage.SaveUserConfiguration(dto.Merge(userConfiguration));
        }

        [HttpGet("app-theme")]
        public async Task<ApplicationThemeDto> GetApplicationTheme()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var globalConfiguration = await _storage.GetGlobalConfiguration();
            if (user == null)
            {
                return new ApplicationThemeDto
                {
                    Theme = globalConfiguration.DefaultTheme
                };
            }

            var userConfiguration = await _storage.GetConfigurationForUser(user.Id);
            return new ApplicationThemeDto
            {
                Theme = userConfiguration.DisplayConfiguration.Theme ?? globalConfiguration.DefaultTheme

            };
        }

        [HttpPost("app-theme")]
        [Authorize]
        public async Task SetApplicationTheme([FromBody] ApplicationThemeDto dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id, AccessMode.Write);
            userConfiguration.DisplayConfiguration.Theme = dto.Theme;
            await _storage.SaveUserConfiguration(userConfiguration);
        }

        [HttpGet("language")]
        public async Task<LanguageDto> GetLanguage()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var globalConfiguration = await _storage.GetGlobalConfiguration();
            if (user == null)
            {
                var locale = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                var supportedLanguages = new[] {"en", "de"};
                return new LanguageDto()
                {
                    Language = supportedLanguages.Contains(locale) ? locale : globalConfiguration.DefaultLanguage
                };
            }

            var userConfiguration = await _storage.GetConfigurationForUser(user.Id);
            return new LanguageDto
            {
                Language = userConfiguration.LocalisationConfiguration.Language ?? "de"

            };
        }

        [HttpPost("language")]
        [Authorize]
        public async Task SetLanguage([FromBody] LanguageDto dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id, AccessMode.Write);
            userConfiguration.LocalisationConfiguration.Language = dto.Language;
            await _storage.SaveUserConfiguration(userConfiguration);
        }

        [HttpGet("first-time")]
        [Authorize]
        public async Task<bool> IsFirstTime()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id);
            return userConfiguration.FirstSignIn;
        }

        [HttpGet("first-time-complete")]
        [Authorize]
        public async Task FirstTimeCompleted()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id, AccessMode.Write);
            userConfiguration.FirstSignIn = false;
            await _storage.SaveUserConfiguration(userConfiguration);
        }


        [HttpPost("data-password")]
        [Authorize]
        public async Task<IActionResult> SaveDataPassword([FromBody] DataPasswordDto request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id, AccessMode.Write);
            if (userConfiguration.HashedDataPassword != null)
            {
                await _storage.SaveUserConfiguration(userConfiguration);
                return BadRequest();
            }

            userConfiguration = _configurationService.SetDataKeys(request, userConfiguration);

            await _storage.SaveUserConfiguration(userConfiguration);
            return Ok();
        }

        [HttpPut("data-password")]
        [Authorize]
        public async Task<IActionResult> ReplaceDataPassword([FromBody] ReplaceDataPasswordDto request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id, AccessMode.Write);
            try
            {
                _configurationService.ReplaceDataPassword(userConfiguration, request.NewDataPassword,
                    request.RecoveryKey);
            }
            catch (Exception)
            {
                return BadRequest("Could not replace the data password");
            }
            finally
            {
                await _storage.SaveUserConfiguration(userConfiguration);
            }

            return Ok();
        }

        [HttpPost("recovery-key")]
        [Authorize]
        public async Task<IActionResult> ReplaceRecoverKey([FromBody] ReplaceDataPasswordDto request)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id);
            try
            {
                _configurationService.ReplaceRecoveryKey(userConfiguration, request.NewDataPassword,
                    request.RecoveryKey);
            }
            catch (Exception)
            {
                return BadRequest("Could not replace the recovery key");
            }
            finally
            {
                await _storage.SaveUserConfiguration(userConfiguration);
            }

            return Ok();
        }

        [HttpGet("data-password")]
        [Authorize]
        public async Task<bool> HasDataPassword()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id);
            return userConfiguration.HashedDataPassword != null;
        }
        
        [HttpGet("dashboard")]
        [Authorize]
        public async Task<DashboardConfiguration[]> LoadDashboardConfiguration()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id);
            return userConfiguration.DashboardConfigurations;
        }
        
        [HttpPost("dashboard")]
        [Authorize]
        public async Task UpdateDashboardConfiguration([FromBody] DashboardConfiguration[] newConfiguration)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id, AccessMode.Write);
            userConfiguration.DashboardConfigurations = newConfiguration;
            await _storage.SaveUserConfiguration(userConfiguration);
        }
        
        [HttpGet("programme")]
        [Authorize]
        public async Task<ProgrammeDefinition[]> LoadProgrammeConfiguration()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id);
            return userConfiguration.ProgrammeConfiguration;
        }
        
        [HttpPost("programme")]
        [Authorize]
        public async Task UpdateProgrammeConfiguration([FromBody] ProgrammeDefinition[] newConfiguration)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id, AccessMode.Write);
            userConfiguration.ProgrammeConfiguration = newConfiguration;
            await _storage.SaveUserConfiguration(userConfiguration);
        }
        
        [HttpGet("reminder")]
        [Authorize]
        public async Task<ReminderConfiguration> LoadReminderConfiguration()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id);
            return userConfiguration.ReminderConfiguration;
        }
        
        [HttpPost("reminder")]
        [Authorize]
        public async Task UpdateReminderConfiguration([FromBody] ReminderConfiguration newConfiguration)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var userConfiguration = await _storage.GetConfigurationForUser(user.Id, AccessMode.Write);
            userConfiguration.ReminderConfiguration = newConfiguration;
            await _storage.SaveUserConfiguration(userConfiguration);
        }

    }
}