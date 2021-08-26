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
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.File;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace LotsenApp.Client.Configuration.File
{
    public class ConfigurationFileUtility: IConfigurationUtility
    {
        
        private readonly IDataProtectionProvider _provider;

        public ConfigurationFileUtility(IDataProtectionProvider provider)
        {
            _provider = provider;
        }
        
        public UserConfiguration ReadUserConfiguration(string userId, string fileName, AccessMode accessMode = AccessMode.Read)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            var lockSlim = ConcurrentFileAccessHelper.GetAccessor(fileName);
            if (accessMode == AccessMode.Read)
            {
                lockSlim.EnterReadLock();
            }
            else
            {
                lockSlim.EnterWriteLock();
            }
            if (!System.IO.File.Exists(fileName))
            {
                if (accessMode == AccessMode.Read)
                {
                    lockSlim.ExitReadLock();
                }
                return new UserConfiguration();
            }
            var text = System.IO.File.ReadAllText(fileName);
            var protector = _provider.CreateProtector(ConfigurationConstants.ConfigurationPurpose + userId);
            text = protector.Unprotect(text);
            if (accessMode == AccessMode.Read)
            {
                lockSlim.ExitReadLock();
            }

            return JsonConvert.DeserializeObject<UserConfiguration>(text);
        }

        public void SaveUserConfiguration(UserConfiguration configuration, string userId, string fileName)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            var file = fileName;
            var writeLock = ConcurrentFileAccessHelper.GetAccessor(file);
            if (!writeLock.IsWriteLockHeld)
            {
                throw new InvalidOperationException("The write lock is not held and writing to the file is not safe");
            }
            var serializedConfiguration = JsonConvert.SerializeObject(configuration);
            
            var protector = _provider.CreateProtector(ConfigurationConstants.ConfigurationPurpose + userId);
            serializedConfiguration = protector.Protect(serializedConfiguration);
            System.IO.File.WriteAllText(file, serializedConfiguration);
            writeLock.ExitWriteLock();
        }

        public GlobalConfiguration ReadGlobalConfiguration(AccessMode accessMode = AccessMode.Read, string fileName = ConfigurationConstants.GlobalConfigurationFile)
        {
            var lockSlim = ConcurrentFileAccessHelper.GetAccessor(fileName);
            if (accessMode == AccessMode.Read)
            {
                lockSlim.EnterReadLock();
            }
            else
            {
                lockSlim.EnterWriteLock();
            }
            if (!System.IO.File.Exists(fileName))
            {
                if (accessMode == AccessMode.Read)
                {
                    lockSlim.ExitReadLock();
                }
                return new GlobalConfiguration();
            }
            var text = System.IO.File.ReadAllText(fileName);
            var protector = _provider.CreateProtector(ConfigurationConstants.GlobalConfigurationPurpose);
            text = protector.Unprotect(text);
            if (accessMode == AccessMode.Read)
            {
                lockSlim.ExitReadLock();
            }

            return JsonConvert.DeserializeObject<GlobalConfiguration>(text);
        }
        
        public void SaveGlobalConfiguration(GlobalConfiguration configuration, string fileName = ConfigurationConstants.GlobalConfigurationFile)
        {
            var writeLock = ConcurrentFileAccessHelper.GetAccessor(fileName);
            if (!writeLock.IsWriteLockHeld)
            {
                throw new InvalidOperationException("The write lock is not held and writing to the file is not safe");
            }
            var serializedConfiguration = JsonConvert.SerializeObject(configuration);
            var protector = _provider.CreateProtector(ConfigurationConstants.GlobalConfigurationPurpose);
            serializedConfiguration = protector.Protect(serializedConfiguration);
            var file = new FileInfo(fileName);
            var directoryName = file.Directory?.FullName;
            if (directoryName != null)
            {
                Directory.CreateDirectory(directoryName);                
            }
            System.IO.File.WriteAllText(fileName, serializedConfiguration);
            writeLock.ExitWriteLock();
        }
    }
}