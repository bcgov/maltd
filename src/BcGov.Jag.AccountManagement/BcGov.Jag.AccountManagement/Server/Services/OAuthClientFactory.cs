using BcGov.Jag.AccountManagement.Server.Models.Configuration;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class OAuthClientFactory : IOAuthClientFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Initializes a new instance of the <see cref="OAuthClientFactory" /> class.</summary>
    /// <param name="serviceProvider">The service provider.</param>
    public OAuthClientFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>Creates an <see cref="IOAuthClient" /> for the given project.</summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    /// <remarks>This only works with Dynamics resources due to hard coded HttpClientFactory factory names.</remarks>
    public IOAuthClient Create(ProjectConfiguration project)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));

        // OAuth is always dynamics
        string projectResourceKey = project.Id + "-dynamics";

        var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
        HttpClient httpClient = httpClientFactory.CreateClient(projectResourceKey + "-authorization");

        // create the handler that will authenticate the call and add authorization header
        ILogger<OAuthClient> clientLogger = _serviceProvider.GetRequiredService<ILogger<OAuthClient>>();
        var oauthClient = new OAuthClient(httpClient, clientLogger);

        return oauthClient;
    }
}
