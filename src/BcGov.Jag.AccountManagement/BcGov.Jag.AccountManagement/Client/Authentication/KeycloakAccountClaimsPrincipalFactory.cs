using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;

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
                    string token = accessToken.Value;
                    string[] parts = token.Split('.');
                    var json = FromBase64UrlString(parts[1]);

                    JsonNode accessTokenNode = JsonNode.Parse(json)!;

                    JsonArray? roles = accessTokenNode["resource_access"]?[resourceName]?["roles"]?.AsArray();
                    if (roles is not null)
                    {
                        foreach (var roleNode in roles)
                        {
                            var role = roleNode?.GetValue<string>();
                            if (role is not null)
                            {
                                identity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }
                        }
                    }
                }
            }
        }

        return principal;
    }

    private static string FromBase64UrlString(string value)
    {
        value = value.Replace('-', '+');
        value = value.Replace('_', '/');
        switch (value.Length % 4)
        {
            case 2: value += "=="; break;
            case 3: value += "="; break;
        }
        var data = Convert.FromBase64String(value);
        var json = UTF8Encoding.UTF8.GetString(data);
        return json;
    }

    private string GetResourceName()
    {
        var section = _configuration.GetRequiredSection("oidc");
        string? resourceName = section.GetValue<string>("ClientId");

        return resourceName ?? string.Empty;
    }
}
