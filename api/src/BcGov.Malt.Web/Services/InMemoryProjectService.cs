using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;

namespace BcGov.Malt.Web.Services
{
    /// <summary>
    /// An in memory version of the project service used for testing.
    /// </summary>
    public class InMemoryProjectService : IProjectService
    {
        private readonly List<Project> _projects = new List<Project>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryProjectService"/> class.
        /// </summary>
        public InMemoryProjectService()
        {
            for (int i = 0; i < 7; i++)
            {
                _projects.Add(new Project { Id = Guid.NewGuid().ToString("n"), Name = $"System {i+1}", Type = "Dynamics" });
            }

            for (int i = 0; i < 7; i++)
            {
                _projects.Add(new Project { Id = Guid.NewGuid().ToString("n"), Name = $"System {i + 1}", Type = "SharePoint" });
            }
        }

        /// <summary>
        /// Gets the list of available projects.
        /// </summary>
        /// <returns>The list of available projects</returns>
        public Task<List<Project>> GetProjectsAsync()
        {
            // return a copy of original list
            return Task.FromResult(new List<Project>(_projects)); 
        }
    }
}
