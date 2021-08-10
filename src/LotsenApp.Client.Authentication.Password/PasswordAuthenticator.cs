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

using LotsenApp.Client.Authentication.Api;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LotsenApp.Client.Authorization;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using HttpRequestException = System.Net.Http.HttpRequestException;

namespace LotsenApp.Client.Authentication.Password
{
    public class PasswordAuthenticator
    {
        private readonly IConfigurationStorage _configurationStorage;
        private readonly TokenValidator _tokenValidator;
        private readonly HttpClient _client;
        private readonly PasswordAuthenticationContext _offlineContext;
        private readonly TokenStorage _tokenStorage;
        private readonly UserManager<LocalLotsenAppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public User User { get; private set; }

        public PasswordAuthenticator(IConfigurationStorage configurationStorage, 
            TokenValidator tokenValidator, 
            HttpClient client, 
            PasswordAuthenticationContext offlineContext,
            TokenStorage tokenStorage,
            UserManager<LocalLotsenAppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _configurationStorage = configurationStorage;
            _tokenValidator = tokenValidator;
            _client = client;
            _offlineContext = offlineContext;
            _tokenStorage = tokenStorage;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<(string userId, bool onlineAuthenticated)> Authenticate(PasswordAuthenticationRequest request, string server, string requestUserId = null)
        {
            // Use the local user in offline mode
            // if (_globalConfiguration.GetConfigValueOrDefault(ConfigurationConstants.ModeConfigurationKey,
            //     ApplicationMode.Offline) == ApplicationMode.Offline && !request.EnforceOnline)
            // {
            //     var user = await _userManager.FindByNameAsync(Environment.UserName);
            //     User = new User(user.Id, user.UserName);
            //     var roles = await _userManager.GetRolesAsync(user);
            //     foreach (var role in roles)
            //     {
            //         User.Roles.Add(role);
            //     }
            //     return (user.Id, false);
            // }

            // _globalConfiguration.SetConfigValue(AuthenticationConstants.LastServerUrlConfigKey, server);
            // _globalConfiguration.SetConfigValue(AuthenticationConstants.LastUsernameConfigKey, request.Username);

            // _client.BaseAddress = new Uri(server);
            // If the lotsen app server is available and online mode exists
            if (await _client.IsAvailable(server) || request.EnforceOnline)
            {
                // Do an online authentication
                var response = await RequestAuthentication(request, server);
                await HandleResponse(response.Token, server, requestUserId);
                // Store the encrypted user information for offline authentication
                await StoreOfflineInformation(request, requestUserId);
                // Add server to list of known servers
                var globalConfiguration = await _configurationStorage.GetGlobalConfiguration(AccessMode.Write);
                var servers = globalConfiguration.KnownServers;
                var set = servers.ToHashSet(new ServerConfigurationEqualityComparer());
                var serverCandidate = new ServerConfiguration
                {
                    Address = server
                };
                set.Add(serverCandidate);
                globalConfiguration.KnownServers = set.ToList();
                await _configurationStorage.SaveGlobalConfiguration(globalConfiguration);
                return (User.UserId, true);
            }

            // If the server is not available on a forced online authentication
            if (request.EnforceOnline)
            {
                // throw an exception
                throw new ServerNotFoundException();
            }

            // Otherwise do an offline authentication. The user was authenticated online at least once, if we are at this point
            // var storage = await _offlineContext.OfflineAuthentications.FindAsync(request.Username);
            var lotsenAppUser = await _userManager.FindByNameAsync(request.Username);
            if (await _userManager.CheckPasswordAsync(lotsenAppUser, request.Password))
            {
                User = new User(lotsenAppUser.Id, lotsenAppUser.UserName);
                var roles = await _userManager.GetRolesAsync(lotsenAppUser);
                User.Roles.AddRange(roles);
                return (User.UserId, false);
            }
            
            //if (storage == null)
            //{
            //    throw new OfflineAuthenticationNotAvailableException();
            //}
            //if (OneWayHashFunction.Verify(JsonConvert.SerializeObject(request),
            //    storage.EncryptedAuthenticationRequest))
            //{
            //    // The user is not set as there is no information besides the user id
            //    return (storage.UserId, true);
            //}

            
            
            throw new BadRequestException();


        }

        private async Task<AuthenticationResponse> RequestAuthentication(PasswordAuthenticationRequest request, string server)
        {
            using var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.PostAsync($"{server}api/v1/authentication/password", content);

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
            catch (HttpRequestException)
            {
                throw new ServerUnreachableException();
            }
            catch (Exception)
            {
                throw new InternalServerException();
            }
        }

        private async Task HandleResponse(string responseToken, string server, string requestUserId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            // Token validation
            await _tokenValidator.ValidateToken(responseToken, server);
            var refreshToken = tokenHandler.ReadJwtToken(responseToken);

            User = new User(AuthenticationConstants.FindClaimValueOrThrow(refreshToken, "nameid"),
                AuthenticationConstants.FindClaimValue(refreshToken, "unique_name", "<unknown_user>"));
            var roles = AuthenticationConstants.FindClaimValues(refreshToken, "role");
            User.Roles.AddRange(roles);
            foreach (var role in roles)
            {
                var identityRole = await _roleManager.FindByNameAsync(role);
                if(identityRole != null)
                {
                    continue;
                }

                await _roleManager.CreateAsync(new IdentityRole(role));
            }
            _tokenStorage.SetRefreshToken(requestUserId ?? User.UserId, responseToken);
        }

        private async Task StoreOfflineInformation(PasswordAuthenticationRequest request, string requestUserId)
        {
            //var hashedRequest = OneWayHashFunction.Hash(JsonConvert.SerializeObject(request));
            //var offlineAuthentication = await
            //    _offlineContext.OfflineAuthentications.FirstOrDefaultAsync(a => a.Username == request.Username);
            //if (offlineAuthentication != null)
            //{
            //    offlineAuthentication.EncryptedAuthenticationRequest = hashedRequest;
            //    _offlineContext.OfflineAuthentications.Update(offlineAuthentication);
            //}
            //else
            //{
            //    await _offlineContext.OfflineAuthentications.AddAsync(new OfflinePasswordAuthenticationModel
            //    {
            //        Username = request.Username,
            //        EncryptedAuthenticationRequest = hashedRequest,
            //        UserId = User.UserId
            //    });
            //}
            
            //await _offlineContext.SaveChangesAsync();

            var lotsenAppUser = await _userManager.FindByIdAsync(requestUserId ?? User.UserId);

            if (lotsenAppUser == null)
            {
                return;
            }

            if (await _userManager.HasPasswordAsync(lotsenAppUser))
            {
                var changeToken = await _userManager.GeneratePasswordResetTokenAsync(lotsenAppUser);
                await _userManager.ResetPasswordAsync(lotsenAppUser, changeToken, request.Password);
            }
            else
            {
                await _userManager.AddPasswordAsync(lotsenAppUser, request.Password);
            }
        }

    }
}
