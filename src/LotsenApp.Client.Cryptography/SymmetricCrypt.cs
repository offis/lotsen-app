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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LotsenApp.Client.Cryptography
{
    [Obsolete]
    [ExcludeFromCodeCoverage]
    internal class SymmetricCrypt
    {
        private const int SymmetricKeyByteLength = 256;
        private const int Rfc2898KeygenIterations = 1024;

        // Wikipedia: "The design and strength of all key lengths of the AES algorithm 
        // (i.e., 128, 192 and 256) are sufficient to protect classified information up to the SECRET level."
        private const int AesKeySizeInBits = 128;
        private const int SaltLength = 16;

        public string Keyword { get; }

        public SymmetricCrypt(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                throw new ArgumentException("The keyword must not be null or empty", "keyword");
            }

            Keyword = keyword;
        }

        public static SymmetricCrypt Generate()
        {
            return new SymmetricCrypt(GenerateRandomSymmetricKey(SymmetricKeyByteLength));
        }

        public async Task<SymmetricEncryptedString> EncryptAsync(string text)
        {
            if( text == null )
            {
                return null;
            }

            try
            {

                using Aes aes = Aes.Create();

                var keyStrengthInBytes = aes.KeySize / 8;
                var salt = GenerateSalt();
                var rfc2898 = new Rfc2898DeriveBytes(Keyword, salt, Rfc2898KeygenIterations);

                aes.Key = rfc2898.GetBytes(keyStrengthInBytes);
                aes.IV = rfc2898.GetBytes(aes.BlockSize / 8);
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                await using var ms = new MemoryStream();
                await using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                await using var writer = new StreamWriter(cs);
                await writer.WriteAsync(text);
                await writer.FlushAsync();
                writer.Close();

                var encryptedTextBytes = ms.ToArray();
                return new SymmetricEncryptedString(BitConverterHelper.ToBase64String(salt), BitConverterHelper.ToBase64String(encryptedTextBytes));
            }
            catch (Exception ex)
            {
                throw new Exception("Could not decrypt", ex);
            }
        }

        public async Task<string> DecryptAsync(SymmetricEncryptedString encryptedText)
        {
            if (encryptedText == null)
            {
                return null;
            }

            try
            {
                var salt = BitConverterHelper.FromBase64String(encryptedText.Salt);
                var encryptedTextBytes = BitConverterHelper.FromBase64String(encryptedText.EncryptedString);

                using Aes aes = Aes.Create();

                var keyStrengthInBytes = aes.KeySize / 8;
                var rfc2898 = new Rfc2898DeriveBytes(Keyword, salt, Rfc2898KeygenIterations);

                aes.Key = rfc2898.GetBytes(keyStrengthInBytes);
                aes.IV = rfc2898.GetBytes(aes.BlockSize / 8);//rfc2898.GetBytes(aes.BlockSize / 8);
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                await using var ms = new MemoryStream(encryptedTextBytes);
                await using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var streamReader = new StreamReader(cs);
                return await streamReader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Could not decrypt", ex);
            }
        }

        private static byte[] GenerateSalt()
        {
            var salt = new byte[SaltLength];
            var rnd = new Random();
            rnd.NextBytes(salt);
            return salt;
        }

        private static string GenerateRandomSymmetricKey(int byteLength)
        {
            var keyBytes = new byte[byteLength];
            var rnd = new Random();
            rnd.NextBytes(keyBytes);
            return Convert.ToBase64String(keyBytes);
        }
    }
}
