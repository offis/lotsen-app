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
using System.Security.Cryptography;
using System.Text;

namespace LotsenApp.Client.Cryptography
{
    public static class PersistentAsymmetricCryptography
    {
        /// <summary>
        /// Supports encryption length of up to 446 bytes.
        /// </summary>
        /// <returns></returns>
        public static (string publicKey, string privateKey) CreateKeyPair()
        {
            using var rsa = RSA.Create(4096);
            return (BitConverterHelper.ToBase64String(rsa.ExportRSAPublicKey()),
                BitConverterHelper.ToBase64String(rsa.ExportRSAPrivateKey()));
        }

        public static string Encrypt(string value, string publicKey)
        {
            var publicKeyBytes = BitConverterHelper.FromBase64String(publicKey);
            using var rsa = RSA.Create(4096);
            rsa.ImportRSAPublicKey(publicKeyBytes, out _);
            var bytes = BitConverterHelper.GetBytes(value);
            var encryptedBytes = rsa.Encrypt(bytes, RSAEncryptionPadding.OaepSHA512);
            return BitConverterHelper.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string value, string privateKey)
        {
            var privateKeyBytes = BitConverterHelper.FromBase64String(privateKey);
            using var rsa = RSA.Create(4096);
            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            var bytes = BitConverterHelper.FromBase64String(value);
            var decryptedBytes = rsa.Decrypt(bytes, RSAEncryptionPadding.OaepSHA512);
            return BitConverterHelper.GetString(decryptedBytes);
        }
    }
}