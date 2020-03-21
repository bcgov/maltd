using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public class TokenAuthorizationHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;
        public readonly OAuthOptions _options;

        public TokenAuthorizationHandler(ITokenService tokenService, OAuthOptions options)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _options = options;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // get the access token and add it to the Authorization header of the request 
            var token = await _tokenService.GetTokenAsync(_options, cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
