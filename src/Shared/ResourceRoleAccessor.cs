using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;

namespace BcGov.Jag.AccountManagement.Shared;
public static class ResourceRoleAccessor
{
    public static IEnumerable<Claim> GetResourceRolesFromAccessToken(string resourceName, string accessToken)
    {
        ArgumentNullException.ThrowIfNull(resourceName);
        ArgumentNullException.ThrowIfNull(accessToken);

        string token = accessToken;
        string[] parts = token.Split('.');

        if (parts.Length < 2) 
        {
            yield break;
        }

        var json = FromBase64UrlString(parts[1]);

        JsonNode? accessTokenNode = JsonNode.Parse(json);

        if (accessTokenNode is null) 
        {
            yield break;
        }

        JsonArray? roles = accessTokenNode["resource_access"]?[resourceName]?["roles"]?.AsArray();
        if (roles is not null)
        {
            foreach (var roleNode in roles)
            {
                var role = roleNode?.GetValue<string>();
                if (role is not null)
                {
                    yield return new Claim(ClaimTypes.Role, role);
                }
            }
        }
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
}
