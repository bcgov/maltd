using BcGov.Jag.AccountManagement.Server.Models.Authorization;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;

namespace BcGov.Jag.AccountManagement.Server.Services;

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

        HttpResponseMessage? response = null;

        try
        {
            response = await _httpClient.PostAsync(options.AuthorizationUri, content, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            // The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.
            throw new OAuthException("The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.", exception);
        }
        catch (TaskCanceledException exception)
        {
            // The request failed due to timeout.
            throw new OAuthException("The request failed due to timeout.", exception);
        }

        if (!response.IsSuccessStatusCode)
        {
            // {"error":"invalid_client","error_description":"MSIS9607: The \u0027client_id\u0027 parameter in the request is invalid. No registered client is found with this identifier."}
            OAuthResponseError? error = await response.Content.ReadFromJsonAsync(OAuthJsonSerializerContext.Default.OAuthResponseError, cancellationToken);

            _logger.LogError("Error getting OAuth for {Resource} using {ClientId} : {HttpStatus} - {ErrorMessage}", options.Resource, options.ClientId, response.StatusCode, error?.Description);
            throw new OAuthClientException(error, options.Resource, options.ClientId);
        }

        var token = await response.Content.ReadFromJsonAsync(OAuthJsonSerializerContext.Default.Token, cancellationToken);
        return token;

    }
}
