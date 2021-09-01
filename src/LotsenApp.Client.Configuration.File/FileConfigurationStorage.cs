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
using System.IO;
using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.File;

namespace LotsenApp.Client.Configuration.File
{
    public class FileConfigurationStorage: IConfigurationStorage
    {
        private readonly ConfigurationFileUtility _utility;
        private readonly IFileService _fileService;

        public FileConfigurationStorage(ConfigurationFileUtility utility, IFileService fileService)
        {
            _utility = utility;
            _fileService = fileService;
            // Task.WaitAll(Task.Run(async () =>
            // {
            //     var globalConfiguration = await GetGlobalConfiguration(AccessMode.Write);
            //     await SaveGlobalConfiguration(globalConfiguration);
            // }));
        }

        private string GetConfigurationFileName(string userId)
        {
            var directory = _fileService.Join($"data/{userId}");
            Directory.CreateDirectory(directory);
            return directory + "/configuration.crypt";
        }
        
        public Task<UserConfiguration> GetConfigurationForUser(string userId, AccessMode accessMode = AccessMode.Read)
        {
            var fileName = GetConfigurationFileName(userId);
            var userConfiguration = _utility.ReadConfiguration<UserConfiguration>(accessMode, userId, fileName);
            userConfiguration.UserId = userId;
            return Task.FromResult(userConfiguration);
        }

        public Task SaveUserConfiguration(UserConfiguration configuration)
        {
            var fileName = GetConfigurationFileName(configuration.UserId);
            _utility.SaveConfigurationFile(configuration, configuration.UserId, fileName);
            return Task.CompletedTask;
        }

        public Task<GlobalConfiguration> GetGlobalConfiguration(AccessMode accessMode = AccessMode.Read)
        {
            var configurationFile = _fileService.Join("config/global.json");
            try
            {
                var globalConfiguration = _utility.ReadConfiguration<GlobalConfiguration>(accessMode, null,
                    configurationFile);
                return Task.FromResult(globalConfiguration);
            }
            catch(Exception)
            {
                var accessor = ConcurrentFileAccessHelper.GetAccessor(configurationFile);
                if (accessor.IsWriteLockHeld)
                {
                    accessor.ExitWriteLock();
                }
                else
                {
                    accessor.ExitReadLock();
                }
            }

            return Task.FromResult(new GlobalConfiguration());
            // throw new Exception("There was an error accessing the global configuration");
        }

        public Task SaveGlobalConfiguration(GlobalConfiguration configuration)
        {
            _utility.SaveConfigurationFile(configuration, null, _fileService.Join("config/global.json"));
            return Task.CompletedTask;
        }
    }
}