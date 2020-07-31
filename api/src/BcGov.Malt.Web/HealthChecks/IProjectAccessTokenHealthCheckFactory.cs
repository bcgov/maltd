using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.HealthChecks
{
    public interface IProjectAccessTokenHealthCheckFactory
    {
        /// <summary>
        /// Creates a <see cref="ProjectAccessTokenHealthCheck"/> for the specified project.
        /// </summary>
        ProjectAccessTokenHealthCheck Create(ProjectConfiguration project);

    }
}