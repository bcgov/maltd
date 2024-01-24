using BcGov.Jag.AccountManagement.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System.Security.Claims;

namespace BcGov.Jag.AccountManagement.Client.Authentication;

public class KeycloakAccountClaimsPrincipalFactory<TAccount> : AccountClaimsPrincipalFactory<TAccount> where TAccount : RemoteUserAccount
{
    private readonly IConfiguration _configuration;

    public KeycloakAccountClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor, IConfiguration configuration) : base(accessor)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(TAccount account, RemoteAuthenticationUserOptions options)
    {
        ClaimsPrincipal principal = await base.CreateUserAsync(account, options);

        if (principal.Identity is ClaimsIdentity identity)
        {
            string resourceName = GetResourceName();

            if (!string.IsNullOrEmpty(resourceName))
            {
                // extract the roles from the access JWT access token
                var accessTokenResult = await TokenProvider.RequestAccessToken();
                if (accessTokenResult.Status == AccessTokenResultStatus.Success && accessTokenResult.TryGetToken(out AccessToken accessToken))
                {
                    var roles = ResourceRoleAccessor.GetResourceRolesFromAccessToken(resourceName, accessToken.Value);
                    identity.AddClaims(roles);
                }
            }
        }

        return principal;
    }

    private string GetResourceName()
    {
        var section = _configuration.GetRequiredSection("oidc");
        string? resourceName = section.GetValue<string>("ClientId");

        return resourceName ?? string.Empty;
    }
}
