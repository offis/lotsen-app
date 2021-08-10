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
using LotsenApp.Client.Cryptography;

namespace LotsenApp.Client.Authentication.DataPassword
{
    public class DataPasswordService
    {
        private readonly IDictionary<string, string> _passwordHashes = new ConcurrentDictionary<string, string>();
        public void SetDataPassword(string userId, string dataPassword)
        {
            if (_passwordHashes.ContainsKey(userId))
            {
                _passwordHashes.Remove(userId);
            }
            
            // Do not store the data password unencrypted
            var encryptedDataPassword = TransientAsymmetricCryptography.Encrypt(dataPassword);
            _passwordHashes.Add(userId, encryptedDataPassword);
        }

        public void RemoveDataPassword(string userId)
        {
            if (_passwordHashes.ContainsKey(userId))
            {
                _passwordHashes.Remove(userId);
            }
        }

        public string GetDataPassword(string userId)
        {
            if (!_passwordHashes.ContainsKey(userId))
            {
                throw new Exception("The user has not verified its data password");
            }
                
            var encryptedDataPassword = _passwordHashes[userId];
            return TransientAsymmetricCryptography.Decrypt(encryptedDataPassword);

        }
    }
}