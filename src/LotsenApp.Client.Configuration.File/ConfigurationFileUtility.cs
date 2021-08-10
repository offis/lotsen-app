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

using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.File;
using LotsenApp.Client.Plugin;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace LotsenApp.Client.Configuration.File
{
    public class ConfigurationFileUtility
    {
        private const string ConfigurationPurpose = "Configuration.File.User.";
        private const string GlobalConfigurationPurpose = "Configuration.File.Global";
        private readonly IDataProtectionProvider _provider;

        public ConfigurationFileUtility(IDataProtectionProvider provider)
        {
            _provider = provider;
        }
        
        public T ReadConfiguration<T>(AccessMode accessMode = AccessMode.Read, string userId = null, string fileName = ConfigurationConstants.GlobalConfigurationFile) where T: new()
        {
            var file = fileName;
            var lockSlim = ConcurrentFileAccessHelper.GetAccessor(file);
            if (accessMode == AccessMode.Read)
            {
                lockSlim.EnterReadLock();
            }
            else
            {
                lockSlim.EnterWriteLock();
            }
            if (!System.IO.File.Exists(file))
            {
                if (accessMode == AccessMode.Read)
                {
                    lockSlim.ExitReadLock();
                }
                return new T();
            }
            var text = System.IO.File.ReadAllText(file);
            if (userId == null)
            {
                var protector = _provider.CreateProtector(GlobalConfigurationPurpose);
                text = protector.Unprotect(text);
            } 
            else 
            {
                var protector = _provider.CreateProtector(ConfigurationPurpose + userId);
                text = protector.Unprotect(text);
            }
            if (accessMode == AccessMode.Read)
            {
                lockSlim.ExitReadLock();
            }

            return JsonConvert.DeserializeObject<T>(text);
        }

        public void SaveConfigurationFile<T>(T configuration, string userId = null, string fileName = ConfigurationConstants.GlobalConfigurationFile)
        {
            var file = fileName;
            var serializedConfiguration = JsonConvert.SerializeObject(configuration);
            
            if (userId == null)
            {
                var protector = _provider.CreateProtector(GlobalConfigurationPurpose);
                serializedConfiguration = protector.Protect(serializedConfiguration);
            }
            else
            {
                var protector = _provider.CreateProtector(ConfigurationPurpose + userId);
                serializedConfiguration = protector.Protect(serializedConfiguration);
            }
            System.IO.File.WriteAllText(file, serializedConfiguration);
            ConcurrentFileAccessHelper.GetAccessor(file).ExitWriteLock();
        }
    }
}