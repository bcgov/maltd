using BcGov.Jag.AccountManagement.Server.Models.Authorization;
using System.Text.Json.Serialization;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class OAuthException : Exception
{
    public OAuthException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected OAuthException(string message) : base(message)
    {
    }
}

public class OAuthClientException : OAuthException
{
    public OAuthResponseError Error { get; init; }
    public Uri Resource { get; init; }
    public string ClientId { get; init; }

    public OAuthClientException(OAuthResponseError error, Uri resource, string clientId) : base(error.Description)
    {
        Error = error;
        Resource = resource;
        ClientId = clientId;
    }
}

/// <summary>
/// Error when an OAuth request returns invalid_client error
/// </summary>
public class InvalidClientOAuthError : FluentResults.Error
{
    public InvalidClientOAuthError(OAuthClientException exception, Guid errorId) : base($"{exception.Error.Description}. Check logs for ErrorId={errorId}")
    {
        this.Metadata.Add("Code", exception.Error.Code);
        this.Metadata.Add("Resource", exception.Resource);
        this.Metadata.Add("ClientId", exception.ClientId);
        this.Metadata.Add("ErrorId", errorId);
    }
}

public class OAuthResponseError
{
    [JsonPropertyName("error")]
    public string Code { get; set; } = String.Empty;

    [JsonPropertyName("error_description")]
    public string Description { get; set; } = String.Empty;
}

[JsonSerializable(typeof(OAuthResponseError))]
[JsonSerializable(typeof(Token))]
public partial class OAuthJsonSerializerContext : JsonSerializerContext
{
}
