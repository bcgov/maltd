using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Services
{
    public class OAuthClient : IOAuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OAuthClient> _logger;

        public OAuthClient(HttpClient httpClient, ILogger<OAuthClient> logger)
        {
            this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            var response = await _httpClient.PostAsync(options.AuthorizationUri, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                // {"error":"invalid_client","error_description":"MSIS9607: The \u0027client_id\u0027 parameter in the request is invalid. No registered client is found with this identifier."}
                var responseData = string.Empty;

                if (response.Content != null)
                {
                    responseData = await response.Content.ReadAsStringAsync();
                }

                //throw new OAuthApiException(
                //    "The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").",
                //    (int)response.StatusCode,
                //    responseData,
                //    response.Headers.ToDictionary(x => x.Key, x => x.Value), null);

                _logger.LogError("Error getting getting OAuth for {Resource} using {ClientId} : {HttpStatus} - {ErrorMessage}", options.Resource, options.ClientId, response.StatusCode, responseData);
                throw new OAuthApiException("Error getting OAuth token", (int)response.StatusCode, responseData, new Dictionary<string, string>(), string.Empty);
            }

            using var stream = await response.Content.ReadAsStreamAsync();

            var token = await JsonSerializer.DeserializeAsync<Token>(stream, cancellationToken: cancellationToken);
            return token;
        }

    }
}
