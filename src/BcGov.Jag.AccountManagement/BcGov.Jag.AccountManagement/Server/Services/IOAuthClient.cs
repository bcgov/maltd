using BcGov.Jag.AccountManagement.Shared.Authorization;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Shared;

namespace BcGov.Jag.AccountManagement.Server.Services;

public interface IOAuthClient
{
    Task<Token> GetTokenAsync(OAuthOptions options, CancellationToken cancellationToken);
}
