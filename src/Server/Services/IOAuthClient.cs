using BcGov.Jag.AccountManagement.Server.Models.Authorization;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;

namespace BcGov.Jag.AccountManagement.Server.Services;

public interface IOAuthClient
{
    /// <summary>
    /// Gets the token.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="OAuthException"></exception>
    /// <exception cref="OAuthClientException"></exception>
    Task<Token> GetTokenAsync(OAuthOptions options, CancellationToken cancellationToken);
}
