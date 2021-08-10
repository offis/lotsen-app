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
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using Newtonsoft.Json;

namespace LotsenApp.Client.Authorization
{
    public class AccessTokenService
    {
        private readonly TokenStorage _storage;
        private readonly TokenValidator _tokenValidator;
        private readonly HttpClient _client;

        public AccessTokenService(TokenStorage storage, TokenValidator tokenValidator, HttpClient client)
        {
            _storage = storage;
            _tokenValidator = tokenValidator;
            _client = client;
        }
        public async Task<string> GetAccessToken(string userId, string capability)
        {
            var accessToken = _storage.GetAccessToken(userId, capability);
            if (IsValid(accessToken))
            {
                return accessToken;
            }

            try
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _storage.GetRefreshToken(userId));

                using var stringContent = new StringContent($"{{ Capability = \"{capability}\" }}");
                var response = await _client.PostAsync("api/v1/authorization/access", stringContent);
                if (response.IsSuccessStatusCode)
                    throw new UnauthorizedAccessException($"Could not get access token for capability '{capability}'");

                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(content);
                // Token validation
                await _tokenValidator.ValidateToken(tokenResponse.Token, _client.BaseAddress.OriginalString);
                _storage.SetAccessToken(userId, capability, tokenResponse.Token);
                return tokenResponse.Token;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"Could not get access token for capability '{capability}'", ex);
            }
        }

        public bool IsValid(string token)
        {
            if (token == null)
            {
                return false;
            }
            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.ReadJwtToken(token);
            return securityToken.ValidTo > DateTime.Now;
        }
    }
}