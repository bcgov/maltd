using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Simple.OData.Client;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class DynamicsODataClientFactory : IODataClientFactory
{
    private static string _dynamicsMetadata;

    public IODataClient Create(string name)
    {

        //_odataClientLogger.LogDebug("Creating IODataClient");

        HttpClient httpClient = new HttpClient();

        ODataClientSettings settings = new ODataClientSettings(httpClient);
        settings.MetadataDocument = GetMetadataDocument();
        settings.IgnoreUnmappedProperties = true;

        ////settings.TraceFilter = ODataTrace.All;
        ////settings.OnTrace = (message, args) => _odataClientLogger.LogInformation(message, args);

        ODataClient oDataClient = new ODataClient(settings);

        //_odataClientLogger.LogDebug("Created IODataClient");
        return oDataClient;
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
