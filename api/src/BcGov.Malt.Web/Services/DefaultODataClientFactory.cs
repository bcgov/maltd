using System.Net.Http;
using BcGov.Malt.Web.Models.Configuration;
using Simple.OData.Client;

namespace BcGov.Malt.Web.Services
{
    public class DefaultODataClientFactory : IODataClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ProjectConfigurationCollection _projects;

        public DefaultODataClientFactory(IHttpClientFactory httpClientFactory, ProjectConfigurationCollection projects)
        {
            _httpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
            _projects = projects ?? throw new System.ArgumentNullException(nameof(projects));
        }

        public IODataClient Create(string name)
        {
            if (name is null)
            {
                throw new System.ArgumentNullException(nameof(name));
            }

            HttpClient httpClient = _httpClientFactory.CreateClient(name + "-dynamics"); // -dynamics is a hack for now

            ODataClientSettings settings = new ODataClientSettings(httpClient);

            ODataClient oDataClient = new ODataClient(settings);

            return oDataClient;
        }
    }
}
