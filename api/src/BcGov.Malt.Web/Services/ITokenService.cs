using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public interface ITokenService
    {
        Task<Token> GetTokenAsync(OAuthOptions configuration, CancellationToken cancellationToken = default(CancellationToken));
    }

}
