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
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace LotsenApp.Client.Authentication.Api
{
    public static class AuthenticationConstants
    {
        public const string LastServerUrlConfigKey = "Login.LastServerUrl";
        public const string LastUsernameConfigKey = "Login.LastUsername";

        public static StringContent ToJsonStringContents<T>(T data)
            => new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");

        public static string FindClaimValue(JwtSecurityToken token, string claimName, string def = null)
            => token.Claims.FirstOrDefault(claim => claim.Type.Equals(claimName))?.Value ?? def;

        public static string FindClaimValueOrThrow(JwtSecurityToken token, string claimName)
            => FindClaimValue(token, claimName) ?? throw new Exception($"Mandatory claim '{claimName}' not found in refresh token");

        public static List<string> FindClaimValues(JwtSecurityToken token, string claimName)
            => token.Claims.Where(claim => claim.Type.Equals(claimName)).Select(claim => claim.Value).ToList();
    }
}