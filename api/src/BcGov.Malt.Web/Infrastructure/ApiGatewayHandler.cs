using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BcGov.Malt.Web.Infrastructure
{
    public class ApiGatewayHandler : DelegatingHandler
    {
        private readonly string _apiGatewayHost;
        private readonly string _apiGatewayPolicy;

        public ApiGatewayHandler(HttpMessageHandler innerHandler, string apiGatewayHost, string apiGatewayPolicy) 
            : base(innerHandler)
        {
            _apiGatewayHost = apiGatewayHost;
            _apiGatewayPolicy = apiGatewayPolicy;
        }

        public ApiGatewayHandler(string apiGatewayHost, string apiGatewayPolicy)
        {
            _apiGatewayHost = apiGatewayHost;
            _apiGatewayPolicy = apiGatewayPolicy;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_apiGatewayHost) && !string.IsNullOrEmpty(_apiGatewayPolicy))
            {
                ApplyPolicy(request);
            }

            return base.SendAsync(request, cancellationToken);
        }

        private void ApplyPolicy(HttpRequestMessage request)
        {
            // save the original host in header
            request.Headers.Add("RouteToHost", request.RequestUri.Host);
            request.RequestUri = ApiGatewayUriBuilder.Build(request.RequestUri, _apiGatewayHost, _apiGatewayPolicy);
        }
    }
}
