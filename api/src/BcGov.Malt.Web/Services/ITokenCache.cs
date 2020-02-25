using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public interface ITokenCache
    {
        Token GetToken(OAuthOptions configuration);

        void SaveToken(OAuthOptions configuration, Token token);
    }
}
