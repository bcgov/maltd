using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Services
{
    public abstract class ResourceUserManagementService : IResourceUserManagementService, IDisposable
    {
        private IDisposable _loggerScope;

        protected ResourceUserManagementService(ProjectConfiguration project, ProjectResource projectResource, ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            ProjectResource = projectResource ?? throw new ArgumentNullException(nameof(projectResource));

            if (project.Resources.All(_ => _ != projectResource))
            {
                throw new ArgumentException("Project resource is not from the project", nameof(projectResource));
            }

            // add the project and resource type to the scope of all requests
            _loggerScope = logger.BeginScope(new Dictionary<string, object>
            {
                ["ProjectName"] = project.Name,
                ["ResourceType"] = projectResource.Type
            });
        }

        protected ProjectConfiguration Project { get; }

        protected ProjectResource ProjectResource { get; }

        public abstract Task<string> AddUserAsync(string username);
        public abstract Task<bool> UserHasAccessAsync(string username);
        public abstract Task<string> RemoveUserAsync(string username);
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_loggerScope != null)
                {
                    _loggerScope.Dispose();
                    _loggerScope = null;
                }
            }
        }

    }
}
