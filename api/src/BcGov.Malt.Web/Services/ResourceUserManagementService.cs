using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;
using Serilog;

namespace BcGov.Malt.Web.Services
{
    public abstract class ResourceUserManagementService : IResourceUserManagementService
    {
        protected ResourceUserManagementService(ProjectConfiguration project, ProjectResource projectResource, ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            ProjectResource = projectResource ?? throw new ArgumentNullException(nameof(projectResource));

            if (project.Resources.All(_ => _ != projectResource))
            {
                throw new ArgumentException("Project resource is not from the project", nameof(projectResource));
            }
            
            Logger = logger
                .ForContext("Project", Destructure(project, projectResource), true);
        }

        /// <summary>
        /// Destructures project and resource for logging.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="projectResource">The project resource.</param>
        /// <returns></returns>
        private static object Destructure(ProjectConfiguration project, ProjectResource projectResource)
        {
            if (projectResource.Type == ProjectType.Dynamics)
            {
                return new
                {
                    project.Name,
                    Resource = new
                    {
                        projectResource.Type,
                        projectResource.ApiGatewayHost,
                        projectResource.ApiGatewayPolicy,
                        projectResource.Resource
                    }
                };
            }

            if (projectResource.Type == ProjectType.SharePoint)
            {
                return new
                {
                    project.Name,
                    Resource = new
                    {
                        projectResource.Type,
                        projectResource.ApiGatewayHost,
                        projectResource.ApiGatewayPolicy,
                        projectResource.RelyingPartyIdentifier
                    }
                };
            }

            return new
            {
                project.Name,
                Resource = new
                {
                    projectResource.Type
                }
            };
        }

        protected ProjectConfiguration Project { get; }

        protected ProjectResource ProjectResource { get; }

        protected ILogger Logger { get; }

        public abstract Task<string> AddUserAsync(string username, CancellationToken cancellationToken);
        public abstract Task<bool> UserHasAccessAsync(string username, CancellationToken cancellationToken);
        public abstract Task<string> RemoveUserAsync(string username, CancellationToken cancellationToken);
    }
}
