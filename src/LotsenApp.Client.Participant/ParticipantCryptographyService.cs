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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.DataPassword;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Cryptography;
using LotsenApp.Client.Participant.Delta;
using LotsenApp.Client.Participant.Model;
using Newtonsoft.Json;

namespace LotsenApp.Client.Participant
{
    [ExcludeFromCodeCoverage]
    public class ParticipantCryptographyService
    {
        private readonly DataPasswordService _dataPasswordService;
        private readonly IConfigurationStorage _configurationStorage;

        public ParticipantCryptographyService(DataPasswordService dataPasswordService, IConfigurationStorage configurationStorage)
        {
            _dataPasswordService = dataPasswordService;
            _configurationStorage = configurationStorage;
        }

        public async Task<IDictionary<string, List<string>>> DecryptHeaderAsync(EncryptedParticipantModel model, string userId)
        {
            var symmetricKey = await GetSymmetricKey(userId);
            string decryptedHeader;
#if RELEASE
            decryptedHeader = SymmetricCryptography.Decrypt(model.EncryptedHeader, symmetricKey);
#endif
#if DEBUG
            decryptedHeader = model.EncryptedHeader;
#endif
            IDictionary<string, List<string>> header;
            try
            {
                // Try new header format
                header = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(decryptedHeader);
            }
            catch (Exception)
            {
                // Fall back to old header format
                var oldHeader = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedHeader);
                header = oldHeader?.ToDictionary(k => k.Key, v => new List<string> {v.Value});
            }
            return header;
        }
        
        public async Task<EncryptedParticipantModel> EncryptHeaderAsync(EncryptedParticipantModel model, string userId, IDictionary<string, List<string>> decryptedHeader)
        {
            var symmetricKey = await GetSymmetricKey(userId);
            var serializedHeader = JsonConvert.SerializeObject(decryptedHeader);
            string encryptedHeader;
#if RELEASE
            encryptedHeader = SymmetricCryptography.Encrypt(serializedHeader, symmetricKey);
#endif
#if DEBUG
            encryptedHeader = serializedHeader;
#endif
            model.EncryptedHeader = encryptedHeader;
            return model;
        }

        public async Task<ParticipantModel> DecryptDataAsync(EncryptedParticipantModel model, string userId)
        {
            var symmetricKey = await GetSymmetricKey(userId);
#if RELEASE
            var decryptedData = SymmetricCryptography.Decrypt(model.EncryptedData, symmetricKey);
            var data = JsonConvert.DeserializeObject<DataBody>(decryptedData);
            return new ParticipantModel(model, data);
#endif
#if DEBUG
            return new ParticipantModel(model, JsonConvert.DeserializeObject<DataBody>(model.EncryptedData));
#endif
        }
        
        public async Task<EncryptedParticipantModel> EncryptDataAsync(ParticipantModel model, string userId)
        {
            var symmetricKey = await GetSymmetricKey(userId);
            var data = JsonConvert.SerializeObject(model.Data);
#if RELEASE
            var encryptedData = SymmetricCryptography.Encrypt(data, symmetricKey);
            
            return new EncryptedParticipantModel(model, null, encryptedData);
#endif
#if DEBUG
            return new EncryptedParticipantModel(model, null, data);
#endif
        }

        public virtual async Task<EncryptedParticipantModel> EncryptModel(ParticipantModel model, string userId)
        {
            var symmetricKey = await GetSymmetricKey(userId);
            var data = JsonConvert.SerializeObject(model.Data);
            var header = JsonConvert.SerializeObject(model.Header);
#if RELEASE
            var encryptedData = SymmetricCryptography.Encrypt(data, symmetricKey);
            var encryptedHeader = SymmetricCryptography.Encrypt(header, symmetricKey);
            return new EncryptedParticipantModel(model, encryptedHeader, encryptedData);
#endif
#if DEBUG
            return new EncryptedParticipantModel(model, header, data);
#endif
        }

        public async Task<EncryptedDeltaFile> EncryptDeltaFile(DeltaFile deltaFile, string userId)
        {
            var symmetricKey = await GetSymmetricKey(userId);
            var serializedDelta = JsonConvert.SerializeObject(deltaFile.Documents);
            var serializedTree = JsonConvert.SerializeObject(deltaFile.DocumentTree);
#if RELEASE
            var encryptedDeltas = SymmetricCryptography.Encrypt(serializedDelta, symmetricKey);
            var encryptedTree = SymmetricCryptography.Encrypt(serializedTree, symmetricKey);
            return new EncryptedDeltaFile(deltaFile, encryptedDeltas, encryptedTree);
#endif
#if DEBUG
            return new EncryptedDeltaFile(deltaFile, serializedDelta, serializedTree);
#endif
        }

        public async Task<DeltaFile> DecryptDeltaFile(EncryptedDeltaFile deltaFile, string userId)
        {
            var symmetricKey = await GetSymmetricKey(userId);
            if (deltaFile.Documents == null)
            {
                return new DeltaFile(deltaFile, new Dictionary<string, DocumentDelta>(), new Dictionary<string, TreeItem>());
            }
#if RELEASE
            var decryptedDeltas = SymmetricCryptography.Decrypt(deltaFile.Documents, symmetricKey);
            var decryptedTree = SymmetricCryptography.Decrypt(deltaFile.DocumentTree, symmetricKey);
            var deserializedDeltas = JsonConvert.DeserializeObject<IDictionary<string, DocumentDelta>>(decryptedDeltas);
            var deserializedTree = JsonConvert.DeserializeObject<IDictionary<string, TreeItem>>(decryptedTree);
            return new DeltaFile(deltaFile, deserializedDeltas, deserializedTree);
#endif
#if DEBUG
            return new DeltaFile(deltaFile, 
                JsonConvert.DeserializeObject<IDictionary<string, DocumentDelta>>(deltaFile.Documents),
                JsonConvert.DeserializeObject<IDictionary<string, TreeItem>>(deltaFile.DocumentTree));
#endif
        }

        private async Task<string> GetSymmetricKey(string userId)
        {
            var dataPassword = _dataPasswordService.GetDataPassword(userId);
            var userConfiguration = await _configurationStorage.GetConfigurationForUser(userId);
            var encryptedPrivateKey = userConfiguration.EncryptedPrivateKeyByDataPassword;
            var privateKey = SymmetricCryptography.Decrypt(encryptedPrivateKey, dataPassword);
            var encryptedDataKey = userConfiguration.EncryptedDataKey;
            return PersistentAsymmetricCryptography.Decrypt(encryptedDataKey, privateKey);
        }
        
    }
}