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

using System.Security.Cryptography;
using System.Text;

namespace LotsenApp.Client.Common
{
    public sealed class PasswordHash
    {
        public string Value { get; }

        private PasswordHash(string hash)
        {
            Value = hash;
        }

        public static PasswordHash Of(string password)
        {
            return new PasswordHash(ComputePasswordHash(password));
        }

        // https://codeshare.co.uk/blog/sha-256-and-sha-512-hash-examples/
        private static string ComputePasswordHash(string inputString)
        {
            var sha512 = SHA512.Create();
            var bytes = Encoding.UTF8.GetBytes(inputString);
            var hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        private static string GetStringFromHash(byte[] hash)
        {
            var result = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(PasswordHash a, PasswordHash b)
        {
            return Equals(a?.Value, b?.Value);
        }

        public static bool operator !=(PasswordHash a, PasswordHash b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is PasswordHash other && other == this;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}