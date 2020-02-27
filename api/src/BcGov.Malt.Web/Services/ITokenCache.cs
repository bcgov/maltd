using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public interface ITokenCache
    {
        /// <summary>
        /// Gets the token associated with the specified configuration.
        /// </summary>
        /// <returns>A token or null if no valid token is available.</returns>
        Token GetToken(OAuthOptions configuration);

        /// <summary>
        /// Saves the token to the cache. Only non-expired tokens are added to the cache.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="token">The token.</param>
        void SaveToken(OAuthOptions configuration, Token token);
    }
}
