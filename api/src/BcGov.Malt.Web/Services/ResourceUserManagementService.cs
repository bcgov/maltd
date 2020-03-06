using System;
using System.Linq;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public abstract class ResourceUserManagementService : IResourceUserManagementService
    {
        protected ResourceUserManagementService(ProjectConfiguration project, ProjectResource projectResource)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            ProjectResource = projectResource ?? throw new ArgumentNullException(nameof(projectResource));

            if (!project.Resources.Any(_ => _ == projectResource))
            {
                throw new ArgumentException("Project resource is not from the project", nameof(projectResource));
            }
        }

        protected ProjectConfiguration Project { get; }

        protected ProjectResource ProjectResource { get; }

        public abstract Task AddUserAsync(string user);
        public abstract Task<bool> UserHasAccessAsync(string user);
        public abstract Task RemoveUserAsync(string user);
    }
}
