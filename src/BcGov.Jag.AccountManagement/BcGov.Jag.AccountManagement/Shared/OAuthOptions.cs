namespace BcGov.Jag.AccountManagement.Shared;

public class OAuthOptions
{
    /// <summary>
    /// The OAuth endpoint that issues the access tokens.
    /// </summary>
    public Uri AuthorizationUri { get; set; }

    /// <summary>
    /// The resource the access token allows access to.
    /// </summary>
    public Uri Resource { get; set; }

    public string Username { get; set; }
    public string Password { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
}
