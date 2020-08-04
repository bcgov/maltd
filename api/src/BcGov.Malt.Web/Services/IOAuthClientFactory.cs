using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    /// <summary>
    /// Represents an interface that can create <see cref="IOAuthClient"/> for a given project configuration/
    /// </summary>
    public interface IOAuthClientFactory
    {
        /// <summary>
        /// Creates an <see cref="IOAuthClient"/> for the given project.
        /// </summary>
        /// <remarks>
        /// This only works with Dynamics resources due to hard coded HttpClientFactory factory names.
        /// </remarks>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        IOAuthClient Create(ProjectConfiguration project);
    }
}