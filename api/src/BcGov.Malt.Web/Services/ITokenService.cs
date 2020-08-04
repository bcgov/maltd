using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    /// <summary>
    /// Provides a service to get OAuth tokens.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Gets an OAuth tokens.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Token> GetTokenAsync(OAuthOptions configuration, CancellationToken cancellationToken = default(CancellationToken));
    }
}
