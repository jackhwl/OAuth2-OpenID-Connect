using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace ImageGallery.Client.Services
{
    public class ImageGalleryHttpClient : IImageGalleryHttpClient
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private HttpClient _httpClient = new HttpClient();

        public ImageGalleryHttpClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public async Task<HttpClient> GetClient()
        {
            string accessToken = string.Empty;

            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;

            // get access token
            //accessToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            // should we renew access & refresh tokens?
            // get expires_at value
            var expires_at = await currentContext.GetTokenAsync("expires_at");

            // compare - make sure to use the exact date formats for comparison
            // (UTC, in this case)

            if (string.IsNullOrWhiteSpace(expires_at) ||
                ((DateTime.Parse(expires_at).AddSeconds(-60)).ToUniversalTime() < DateTime.UtcNow))
            {
                accessToken = await RenewTokens();
            }
            else
            {
                accessToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                _httpClient.SetBearerToken(accessToken);
            }
            _httpClient.BaseAddress = new Uri("https://localhost:44345/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return _httpClient;
        }

        private async Task<string> RenewTokens()
        {
            // get the current HttpContext to access the tokens
            var currentContext = _httpContextAccessor.HttpContext;

            // get the metadata
            var discoveryClient = new DiscoveryClient("https://localhost:44380/");
            var metaDataResponse = await discoveryClient.GetAsync();

            // create a new token client to get new tokens
            var tokenClient = new TokenClient(metaDataResponse.TokenEndpoint, "imagegalleryclient", "secret");
            // get the saved refresh token
            var currentRefreshToken = await currentContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            // refresh the tokens
            var tokenResult = await tokenClient.RequestRefreshTokenAsync(currentRefreshToken);

            if (!tokenResult.IsError)
            {
                // update the tokens & expiration value
                var updatedTokens = new List<AuthenticationToken>();
                updatedTokens.Add(new AuthenticationToken{Name=OpenIdConnectParameterNames.IdToken, Value=tokenResult.IdentityToken});
                updatedTokens.Add(new AuthenticationToken{Name=OpenIdConnectParameterNames.AccessToken, Value=tokenResult.AccessToken});
                updatedTokens.Add(new AuthenticationToken{Name=OpenIdConnectParameterNames.RefreshToken, Value=tokenResult.RefreshToken});

                var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                updatedTokens.Add(new AuthenticationToken{Name="expires_at", Value=expiresAt.ToString("o", CultureInfo.InvariantCulture)});

                // get authenticate result, containing the current principal & properties
                var currentAuthenticateResult = await currentContext.AuthenticateAsync("Cookies");

                // store the updated tokens
                currentAuthenticateResult.Properties.StoreTokens(updatedTokens);

                // sign in 
                await currentContext.SignInAsync("Cookies", currentAuthenticateResult.Principal,
                    currentAuthenticateResult.Properties);

                // return the new access token
                return tokenResult.AccessToken;

                //// Save the tokens.

                //// get auth info
                //var authenticatieInfo = await currentContext.AuthenticateAsync("Cookies");

                //// create a new value for expires_at, and save it
                //var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tokenResult.ExpiresIn);
                //authenticatieInfo.Properties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));
                //authenticatieInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.AccessToken, tokenResult.AccessToken);
                //authenticatieInfo.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, tokenResult.RefreshToken);

                //// we're signing in again with the new values.
                //await currentContext.SignInAsync("Cookies", currentAuthenticateResult.Principal,
                //    currentAuthenticateResult.Properties);

                // return the new access token
                // return tokenResult.AccessToken;
            }
            else
            {
                throw new Exception("Problem encountered while refreshing tokens.", tokenResult.Exception);
            }
        }
    }
}

