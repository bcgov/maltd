using BcGov.Jag.AccountManagement.Server.Models.Configuration;

namespace BcGov.Jag.AccountManagement.Server.HealthChecks;

public interface IProjectAccessTokenHealthCheckFactory
{
    /// <summary>
    /// Creates a <see cref="ProjectAccessTokenHealthCheck"/> for the specified project.
    /// </summary>
    ProjectAccessTokenHealthCheck Create(ProjectConfiguration project);

}
