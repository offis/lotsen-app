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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LotsenApp.Client.Authentication.Api;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Cryptography;
using LotsenApp.Client.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LotsenApp.Client.Authentication.Token
{
    public class TokenAuthenticator
    {
        public User User { get; private set; }
        public bool IsAuthenticated => User != null;
        public Uri ServerUri => _client?.BaseAddress;

        private readonly HttpClient _client;
        private readonly IConfigurationStorage _configurationStorage;
        private readonly TokenValidator _tokenValidator;
        private readonly TokenAuthenticationContext _offlineContext;
        private readonly RoleManager<IdentityRole> _roleManager;

        private JwtSecurityToken _refreshToken;
        private AuthenticationHeaderValue _refreshTokenHeader;

        public TokenAuthenticator(HttpClient client, 
            IConfigurationStorage configurationStorage,
            TokenValidator tokenValidator, 
            TokenAuthenticationContext offlineContext,
            RoleManager<IdentityRole> roleManager)
        {
            _client = client;
            _configurationStorage = configurationStorage;
            _tokenValidator = tokenValidator;
            _offlineContext = offlineContext;
            _roleManager = roleManager;
        }

        public async Task<string> Authenticate(TokenAuthenticationRequest request, LocalLotsenAppUser user)
        {
            var userConfiguration = await _configurationStorage.GetConfigurationForUser(user.Id);
            var (timeLeft, totalTime) = GetExpirationTime(request.RefreshToken);
            // If the lotsen app server is available and token is halfway past its expiration time and we are online
            var server = userConfiguration.SynchronisationConfiguration.Server.FirstOrDefault();
            if (await _client.IsAvailable(server) &&
                (2 * timeLeft) < totalTime)
            {
                // Do an online authentication
                var response = await RequestAuthentication(request);
                _refreshToken = await HandleResponse(response.Token, server);
                // Store the encrypted user information for offline authentication
                await StoreOfflineInformation(request);
            }

            var token = _refreshToken?.RawData ?? request.RefreshToken;
            
            _refreshTokenHeader = new AuthenticationHeaderValue("Bearer", token); // TokenType = Bearer
            _client.DefaultRequestHeaders.Authorization = _refreshTokenHeader;

            return token;
        }

        private async Task<AuthenticationResponse> RequestAuthentication(TokenAuthenticationRequest request)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.RefreshToken);
            using var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/v1/authentication/refresh", content);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ServerNotFoundException();
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new BadRequestException();
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InternalServerException();
            }

            var contents = await response.Content.ReadAsStringAsync();
            var authResponse = JsonConvert.DeserializeObject<AuthenticationResponse>(contents);

            return authResponse;
        }

        private async Task<JwtSecurityToken> HandleResponse(string responseToken, string serverUrl)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // Token validation
            var globalConfiguration = await _configurationStorage.GetGlobalConfiguration();
            var serverConfiguration = globalConfiguration.KnownServers.FirstOrDefault(s => s.Address == serverUrl) ?? new ServerConfiguration
            {
                Address = serverUrl,
                VerificationKey = null
            };
            await _tokenValidator.ValidateToken(responseToken, serverConfiguration.Address);
            var refreshToken = tokenHandler.ReadJwtToken(responseToken);

            var roles = AuthenticationConstants.FindClaimValues(refreshToken, "role");
            User = new User(AuthenticationConstants.FindClaimValueOrThrow(refreshToken, "nameid"),
                AuthenticationConstants.FindClaimValue(refreshToken, "unique_name", "<unknown_user>"));
            User.Roles.AddRange(roles);

            foreach (var role in roles)
            {
                var identityRole = await _roleManager.FindByNameAsync(role);
                if (identityRole != null)
                {
                    continue;
                }

                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            return refreshToken;
        }

        private async Task StoreOfflineInformation(TokenAuthenticationRequest request)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(request.RefreshToken);
            var nameId = AuthenticationConstants.FindClaimValueOrThrow(token, "nameid");
            var hashedRequest = OneWayHashFunction.Hash(JsonConvert.SerializeObject(request));
            // encrypt the refresh token by using the data password
            // Store refresh token encrypted by the data password of a user and scoped to a user
            // var encryptedRefreshToken = await SymmetricEncryptionFunction.EncryptAsync(_refreshToken.RawData, "banana");
            var offlineAuthentication = await
                _offlineContext.OfflineAuthentications.FirstOrDefaultAsync(a => a.UserName == nameId);
            if (offlineAuthentication != null)
            {
                offlineAuthentication.EncryptedToken = hashedRequest;
                _offlineContext.OfflineAuthentications.Update(offlineAuthentication);
            }
            else
            {
                await _offlineContext.OfflineAuthentications.AddAsync(new OfflineTokenAuthenticationModel
                {
                    UserName = nameId,
                    EncryptedToken = hashedRequest,
                });
            }


            await _offlineContext.SaveChangesAsync();
        }

        private (TimeSpan timeLeft, TimeSpan totalTime) GetExpirationTime(string refreshToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(refreshToken);
            var expirationTime = token.ValidTo - token.ValidFrom;
            var currentTime = token.ValidTo - DateTime.UtcNow;
            return (currentTime, expirationTime);
        }

        [Obsolete]
        public async Task Deauthenticate(LocalLotsenAppUser user, string server)
        {
            if (_client.BaseAddress != null)
            {
                _client.BaseAddress = new Uri(server);
            }
            try
            {
                _client.DefaultRequestHeaders.Authorization = _refreshTokenHeader;
                await _client.PostAsync("api/v1/authentication/logout", new StringContent("", Encoding.UTF8, "application/json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during deauthentication: {ex}");
            }
            finally
            {
                User = null;
            }
        }
    }
}
