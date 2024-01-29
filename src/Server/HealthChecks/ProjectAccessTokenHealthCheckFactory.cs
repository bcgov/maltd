using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Services;
using BcGov.Jag.AccountManagement.Server.Services.Sharepoint;

namespace BcGov.Jag.AccountManagement.Server.HealthChecks;

public class ProjectAccessTokenHealthCheckFactory : IProjectAccessTokenHealthCheckFactory
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>Initializes a new instance of the <see cref="ProjectAccessTokenHealthCheckFactory" /> class.</summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is null</exception>
    public ProjectAccessTokenHealthCheckFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Creates a <see cref="ProjectAccessTokenHealthCheck"/> for the specified project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">project</exception>
    public ProjectAccessTokenHealthCheck Create(ProjectConfiguration project)
    {
        if (project == null) throw new ArgumentNullException(nameof(project));

        ISamlAuthenticator samlAuthenticator = _serviceProvider.GetRequiredService<ISamlAuthenticator>();
        IOAuthClientFactory oauthClientFactory = _serviceProvider.GetRequiredService<IOAuthClientFactory>();

        return new ProjectAccessTokenHealthCheck(project, samlAuthenticator, oauthClientFactory);
    }
}
