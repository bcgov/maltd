using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Simple.OData.Client;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class DefaultODataClientFactory : IODataClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ODataClient> _odataClientLogger;

    private static string _dynamicsMetadata;

    public DefaultODataClientFactory(IHttpClientFactory httpClientFactory, ILogger<ODataClient> odataClientLogger)
    {
        _httpClientFactory = httpClientFactory ?? throw new System.ArgumentNullException(nameof(httpClientFactory));
        _odataClientLogger = odataClientLogger ?? throw new ArgumentNullException(nameof(odataClientLogger));
    }

    public IODataClient Create(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        //_odataClientLogger.LogDebug("Creating IODataClient");

        HttpClient httpClient = _httpClientFactory.CreateClient(name);

        ODataClientSettings settings = new ODataClientSettings(httpClient);
        settings.MetadataDocument = GetMetadataDocument();
        settings.IgnoreUnmappedProperties = true;

        ////settings.TraceFilter = ODataTrace.All;
        ////settings.OnTrace = (message, args) => _odataClientLogger.LogInformation(message, args);

        ODataClient oDataClient = new ODataClient(settings);
        
        //_odataClientLogger.LogDebug("Created IODataClient");
        return oDataClient;
    }

    public HttpClient CreateHttpClient(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        HttpClient httpClient = _httpClientFactory.CreateClient(name);
        return httpClient;
    }

    private string GetMetadataDocument()
    {
        if (_dynamicsMetadata == null)
        {
            var assembly = typeof(DefaultODataClientFactory).GetTypeInfo().Assembly;

            var embeddedProvider = new EmbeddedFileProvider(assembly);
            using Stream stream = embeddedProvider.GetFileInfo("Models.Dynamics.metadata.xml").CreateReadStream();
            using StreamReader reader = new StreamReader(stream);

            _dynamicsMetadata = reader.ReadToEnd();
        }

        return _dynamicsMetadata;
    }


}
