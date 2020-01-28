using System.Collections.Generic;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;

namespace BcGov.Malt.Web.Services
{
    /// <summary>
    /// Provides access to the list of available projects.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Gets the list of projects.
        /// </summary>
        /// <returns>A list of projects</returns>
        public Task<List<Project>> GetProjectsAsync();
    }
}
