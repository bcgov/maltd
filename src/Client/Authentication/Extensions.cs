using System.Security.Claims;
using System.Text.Json;

namespace BcGov.Jag.AccountManagement.Client.Authentication;

public static class Extensions
{
    private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    /// <summary>
    /// Parses the Resource Access claim and returns the roles for the given resource
    /// </summary>
    /// <param name="resourceName">The name of the resource to retrive the roles from</param>
    public static IEnumerable<string> GetResourceAccessRoles(this ClaimsIdentity identity, string resourceName)
    {
        var resourceAccessClaim = identity.Claims
            .SingleOrDefault(claim => claim.Type == "resource_access")
            ?.Value;

        if (string.IsNullOrWhiteSpace(resourceAccessClaim))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            var resources = JsonSerializer.Deserialize<Dictionary<string, ResourceAccess>>(resourceAccessClaim, serializerOptions);

            return resources?.TryGetValue(resourceName, out var access) == true
                ? access.Roles
                : Enumerable.Empty<string>();
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    private class ResourceAccess
    {
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }
}
