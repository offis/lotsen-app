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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using LotsenApp.Client.File;
using Microsoft.AspNetCore.DataProtection;

namespace LotsenApp.Client.Authorization
{
    public class TokenStorage
    {
        private readonly IFileService _fileService;
        private const string Purpose = "LotsenApp.Client.TokenStorage";

        private readonly IDictionary<string, IDictionary<string, string>> _accessTokens =
            new ConcurrentDictionary<string, IDictionary<string, string>>();
        private readonly IDataProtector _protector;
        
        public TokenStorage(IDataProtectionProvider provider, IFileService fileService)
        {
            _fileService = fileService;
            Directory.CreateDirectory(fileService.Join("config/tokens"));
            _protector = provider.CreateProtector(Purpose);
        }

        public string GetRefreshToken(string userId)
        {
            try
            {
                var encryptedToken = System.IO.File.ReadAllText(_fileService.Join($"config/tokens/{userId}.token"));
                return _protector.Unprotect(encryptedToken);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public void SetRefreshToken(string userId, string refreshToken)
        {
            var encryptedToken = _protector.Protect(refreshToken);
            System.IO.File.WriteAllText(_fileService.Join($"config/tokens/{userId}.token"), encryptedToken);
        }

        public void SetAccessToken(string userId, string capability, string accessToken)
        {
            if (_accessTokens.ContainsKey(userId))
            {
                var userCapabilities = _accessTokens[userId];
                userCapabilities[capability] = accessToken;
                return;
            }
            var capabilities = new ConcurrentDictionary<string, string>();
            _accessTokens[userId] = capabilities;
            capabilities[capability] = accessToken;
        }

        public string GetAccessToken(string userId, string capability)
        {
            if (_accessTokens.ContainsKey(userId) && _accessTokens[userId].ContainsKey(capability))
            {
                return _accessTokens[userId][capability];
            }
            return null;
        }
    }
}
