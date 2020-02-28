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
            _projects.Add(new Project("Corrections Dev"));
            _projects.Add(new Project("Corrections Test"));
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
