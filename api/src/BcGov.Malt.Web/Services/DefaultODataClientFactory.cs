using System.Net.Http;
using Simple.OData.Client;

namespace BcGov.Malt.Web.Services
{
    public class DefaultODataClientFactory : IODataClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DefaultODataClientFactory(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
        }

        public IODataClient Create(string name)
        {
            if (name is null)
            {
                throw new System.ArgumentNullException(nameof(name));
            }

            HttpClient httpClient = _httpClientFactory.CreateClient(name);

            ODataClientSettings settings = new ODataClientSettings(httpClient);

            ODataClient oDataClient = new ODataClient(settings);

            return oDataClient;
        }
    }
}
