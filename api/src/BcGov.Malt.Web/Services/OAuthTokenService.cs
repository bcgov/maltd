using System;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public class OAuthTokenService : ITokenService
    {
        private readonly IOAuthClient _client;
        private readonly ITokenCache<OAuthOptions, Token> _tokenCache;

        public OAuthTokenService(IOAuthClient client, ITokenCache<OAuthOptions, Token> tokenCache)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
        }


        public async Task<Token> GetTokenAsync(OAuthOptions configuration, CancellationToken cancellationToken)
        {
            Token token = _tokenCache.GetToken(configuration);
            if (token != null && !token.IsAccessTokenExpired)
            {
                return token;
            }

            // TODO: use refresh token if available 
            token = await _client.GetTokenAsync(configuration, cancellationToken);

            _tokenCache.SaveToken(configuration, token, token.AccessTokenExpiresAtUtc);

            return token;
        }
    }
}
