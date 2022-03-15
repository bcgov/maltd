using BcGov.Jag.AccountManagement.Server.Models.Authorization;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;

namespace BcGov.Jag.AccountManagement.Server.Services;

public interface IOAuthClient
{
    Task<Token> GetTokenAsync(OAuthOptions options, CancellationToken cancellationToken);
}
