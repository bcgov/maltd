using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public class OAuthClient : IOAuthClient
    {
        private readonly HttpClient _httpClient;

        public OAuthClient(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async Task<Token> GetTokenAsync(OAuthOptions options, CancellationToken cancellationToken)
        {
            _httpClient.DefaultRequestHeaders.Add("client-request-id", Guid.NewGuid().ToString());
            _httpClient.DefaultRequestHeaders.Add("return-client-request-id", "true");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            var data = new Dictionary<string, string>
                    {
                        {"resource", options.Resource.ToString() },
                        {"client_id", options.ClientId},
                        {"client_secret", options.ClientSecret},
                        {"username", options.Username},
                        {"password", options.Password},
                        {"scope", "openid"},
                        {"response_mode", "form_post"},
                        {"grant_type", "password"}
                    };

            using var content = new FormUrlEncodedContent(data);

            // grab the time before so when computing the exiration,
            // the expiration date/time will be 
            DateTimeOffset tokenCreatedAtUtc = DateTimeOffset.UtcNow;

            var response = await _httpClient.PostAsync(options.AuthorizationUri, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseData = response.Content == null
                    ? null
                    : await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                //throw new OAuthApiException(
                //    "The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").",
                //    (int)response.StatusCode,
                //    responseData,
                //    response.Headers.ToDictionary(x => x.Key, x => x.Value), null);

                throw new OAuthApiException();
            }

            using var stream = await response.Content.ReadAsStreamAsync();

            var token = await JsonSerializer.DeserializeAsync<Token>(stream);

            // compute the absolute expiration time of the access and refresh tokens
            //token.ExpiresAtUtc = tokenCreatedAtUtc.AddSeconds(token.ExpiresIn);
            //token.RefreshTokenExpiresAtUtc = tokenCreatedAtUtc.AddSeconds(token.RefreshTokenExpiresIn);

            return token;

        }

    }
}
