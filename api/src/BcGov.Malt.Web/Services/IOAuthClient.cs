using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public interface IOAuthClient
    {
        Task<Token> GetTokenAsync(OAuthOptions options, CancellationToken cancellationToken);
    }
}