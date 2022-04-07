namespace BcGov.Jag.AccountManagement.Server.Models.Configuration;

public class ProjectResource
{
    /// <summary>
    /// The project type.
    /// </summary>
    public ProjectType Type { get; set; }

    /// <summary>
    /// The OAuth endpoint that issues the access tokens.
    /// </summary>
    public Uri AuthorizationUri { get; set; }

    /// <summary>
    /// The resource the access token is for.
    /// </summary>
    public Uri Resource { get; set; }

    public string Username { get; set; }
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the client identifier used in OAuth authentication.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret used in OAuth authentication.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the relying party identifier used in ADFS SAML based
    /// authentication.
    /// </summary>
    public string RelyingPartyIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the optional API gateway host.
    /// </summary>
    /// <value>
    /// The API gateway host.
    /// </value>
    public string ApiGatewayHost { get; set; }

    /// <summary>
    /// Gets or sets the API Gateway policy. If no policy is configured
    /// the API gateway will not be used.
    /// </summary>
    public string ApiGatewayPolicy { get; set; }
}
