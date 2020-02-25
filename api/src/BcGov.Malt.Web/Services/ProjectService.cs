using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ProjectConfigurationCollection _projects;

        public ProjectService(ProjectConfigurationCollection projects)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        }

        public Task<List<Project>> GetProjectsAsync()
        {
            var projects = _projects
                .Select(_ => new Project(_.Name))
                .ToList();

            return Task.FromResult(projects);
        }

    }

}
