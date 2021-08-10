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

using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api;
using Microsoft.EntityFrameworkCore;

namespace LotsenApp.Client.Configuration.Database
{
    public class DatabaseConfigurationStorage: IConfigurationStorage
    {
        private readonly DatabaseConfigurationContext _context;

        public DatabaseConfigurationStorage(DatabaseConfigurationContext context)
        {
            _context = context;
        }

        public async Task<UserConfiguration> GetConfigurationForUser(string userId, AccessMode accessMode = AccessMode.Read)
        {
            var userConfiguration =  await _context.UserConfigurations.FirstOrDefaultAsync(c => c.UserId == userId);
            if (userConfiguration == null)
            {
                var newUserConfiguration = new UserConfiguration
                {
                    UserId = userId
                };
                await _context.AddAsync(newUserConfiguration);
                await _context.SaveChangesAsync();
                return newUserConfiguration;
            }

            return userConfiguration;
        }

        public async Task SaveUserConfiguration(UserConfiguration configuration)
        {
            _context.UserConfigurations.Update(configuration);
            await _context.SaveChangesAsync();
        }

        public async Task<GlobalConfiguration> GetGlobalConfiguration(AccessMode accessMode = AccessMode.Read)
        {
            var globalConfiguration = await _context.GlobalConfiguration.FirstOrDefaultAsync();
            if (globalConfiguration == null)
            {
                var newGlobalConfiguration = new GlobalConfiguration();
                await _context.GlobalConfiguration.AddAsync(newGlobalConfiguration);
                await _context.SaveChangesAsync();
                return newGlobalConfiguration;
            }

            return globalConfiguration;
        }

        public async Task SaveGlobalConfiguration(GlobalConfiguration configuration)
        {
            _context.GlobalConfiguration.Update(configuration);
            await _context.SaveChangesAsync();
        }
    }
}