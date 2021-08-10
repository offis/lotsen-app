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
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace LotsenApp.Client.Authentication.Api
{
    public class TokenValidator
    {
        private readonly Func<string, Task<SecurityKey>> _lazyKeyCreator;
        public TokenValidator(IConfigurationStorage configurationStorage)
        {
            _lazyKeyCreator = async serverUrl =>
            {
                // Get the verification key for token verification
                var globalConfiguration = await configurationStorage.GetGlobalConfiguration();
                var knownServer =
                    globalConfiguration.KnownServers.FirstOrDefault(s => s.Address == serverUrl);
                if (knownServer == null)
                {
                    using var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
                    {
                        BaseAddress = new Uri(serverUrl),
                        Timeout = TimeSpan.FromSeconds(30),
                    };
                    var remoteVerificationKeyResponse = await httpClient.GetAsync("api/v1/account/authentication/public");
                    if (!remoteVerificationKeyResponse.IsSuccessStatusCode)
                    {
                        throw new Exception("The remote public key cannot be retrieved.");
                    }

                    var response = await remoteVerificationKeyResponse.Content.ReadAsStringAsync();
                    var deserializedResponse = JsonConvert.DeserializeObject<VerificationKeyResponse>(response);
                    var verificationKey = deserializedResponse.PublicKey;
                    knownServer = new ServerConfiguration
                    {
                        Address = serverUrl,
                        VerificationKey = verificationKey
                    };
                    globalConfiguration.KnownServers.Add(knownServer);
                }

                var algorithm = ECDsa.Create(ECCurve.NamedCurves.nistP521);
                algorithm.ImportSubjectPublicKeyInfo(ECDsaCurveParameterExtractor.FromHexString(knownServer.VerificationKey), out var _);
                return ECDsaCurveParameterExtractor.FromECDsa(algorithm);
            };
        }

        /// <summary>
        /// Will throw if an invalid token is present
        /// </summary>
        /// <param name="token"></param>
        /// <param name="serverUrl"></param>
        public async Task ValidateToken(string token, string serverUrl)
        {
            var key = await _lazyKeyCreator.Invoke(serverUrl);
            var tokenHandler = new JwtSecurityTokenHandler();
            // Token validieren
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateAudience = false,
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateTokenReplay = false
            }, out _);
        }
    }
}