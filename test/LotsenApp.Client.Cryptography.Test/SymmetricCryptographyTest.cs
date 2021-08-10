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
using Xunit;

namespace LotsenApp.Client.Cryptography.Test
{
    [ExcludeFromCodeCoverage]
    public class SymmetricCryptographyTest
    {
        [Fact]
        public void TestSaltEncryption()
        {
            const string key = "password";
            const string valueToEncrypt = "test";
            var encryptedValue = SymmetricCryptography.Encrypt(valueToEncrypt, key);
            
            Assert.NotNull(encryptedValue);
        }
        
        [Theory]
        [InlineData("password")]
        [InlineData("passwordThatIsReallyLongButShouldHaveTheSameSaltLengthAsAnyOtherKeyWhatSoEver")]
        [InlineData("12354181238123787126361262")]
        [InlineData("bb25b12a-bf88-432b-bafa-d06ea1f37b37")]
        public void TestSaltLength(string key)
        {
            const string valueToEncrypt = "test";
            var encryptedValue = SymmetricCryptography.Encrypt(valueToEncrypt, key);
            var startSalt = encryptedValue.IndexOf("$", StringComparison.InvariantCulture);
            var endSalt = encryptedValue.LastIndexOf("$", StringComparison.InvariantCulture);
            var salt = encryptedValue.Substring(startSalt + 1, endSalt - startSalt - 1);
            Assert.Equal(44, salt.Length);
        }
        
        [Fact]
        public void TestRoundtrip()
        {
            const string key = "password";
            const string valueToEncrypt = "test";
            var encryptedValue = SymmetricCryptography.Encrypt(valueToEncrypt, key);
            var roundtripText = SymmetricCryptography.Decrypt(encryptedValue, key);
            
            Assert.Equal(valueToEncrypt, roundtripText);
        }

        [Fact]
        public void ShouldCreateKeyPair()
        {
            var result = SymmetricCryptography.CreateKey();
            
            Assert.NotNull(result);
        }
        
        [Fact]
        public void ShouldThrowOnUnknownString()
        {
            Assert.Throws<ArgumentException>(() => SymmetricCryptography.Decrypt("SomeErrorText", "unusedKey"));
        }
    }
}