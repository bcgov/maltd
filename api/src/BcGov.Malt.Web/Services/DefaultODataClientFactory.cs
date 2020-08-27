using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Simple.OData.Client;

namespace BcGov.Malt.Web.Services
{
    public class DefaultODataClientFactory : IODataClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ODataClient> _odataClientLogger;

        public DefaultODataClientFactory(IHttpClientFactory httpClientFactory, ILogger<ODataClient> odataClientLogger)
        {
            _httpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
            _odataClientLogger = odataClientLogger ?? throw new ArgumentNullException(nameof(odataClientLogger));
        }

        public IODataClient Create(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            HttpClient httpClient = _httpClientFactory.CreateClient(name);

            ODataClientSettings settings = new ODataClientSettings(httpClient);

            settings.TraceFilter = ODataTrace.All;
            settings.OnTrace = (message, args) => _odataClientLogger.LogInformation(message, args);

            ODataClient oDataClient = new ODataClient(settings);

            return oDataClient;
        }
    }
}
