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
using LotsenApp.Client.Cryptography;
using LotsenApp.Client.File;
using Newtonsoft.Json;

namespace LotsenApp.Client.Configuration.File
{
    public class FileUserConfigurationService: IUserConfigurationService
    {
        private readonly IFileService _fileService;

        public FileUserConfigurationService(IFileService fileService)
        {
            _fileService = fileService;
        }
        
        internal void BackupDataKeys(UserConfiguration configuration)
        {
            var parentDirectory = _fileService.Join("data/key-backup");
            Directory.CreateDirectory(parentDirectory);
            
            var backup = new UserDataBackupModel
            {
                HashedDataPassword = configuration.HashedDataPassword,
                EncryptedPrivateKeyByDataPassword = configuration.EncryptedPrivateKeyByDataPassword,
                EncryptedPrivateKeyByRecoveryKey = configuration.EncryptedPrivateKeyByRecoveryKey,
                EncryptedDataKey = configuration.EncryptedDataKey
            };
            System.IO.File.WriteAllText($"{parentDirectory}/{configuration.UserId}.backup", JsonConvert.SerializeObject(backup));
        }

        public UserConfiguration SetDataKeys(string dataPassword, string recoveryKey, UserConfiguration userConfiguration)
        {
            BackupDataKeys(IUserConfigurationService.SetDataKeysDefault(dataPassword, recoveryKey, userConfiguration));
            
            return userConfiguration;
        }

        public UserConfiguration ReplaceDataPassword(UserConfiguration userConfiguration,
            string newDataPassword, string recoveryKey)
        {
            BackupDataKeys(IUserConfigurationService.ReplaceDataPasswordDefault(userConfiguration, newDataPassword, recoveryKey));
            return userConfiguration;
        }

        public UserConfiguration ReplaceRecoveryKey(UserConfiguration userConfiguration,
            string dataPassword, string newRecoveryKey)
        {
            BackupDataKeys(IUserConfigurationService.ReplaceRecoveryKeyDefault(userConfiguration, dataPassword, newRecoveryKey));
            return userConfiguration;
        }
    }
}