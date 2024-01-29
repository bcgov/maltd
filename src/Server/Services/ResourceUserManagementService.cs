using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Shared;
using FluentResults;
using ILogger = Serilog.ILogger;

namespace BcGov.Jag.AccountManagement.Server.Services;

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

    public abstract Task<Result<string>> AddUserAsync(User user, CancellationToken cancellationToken);
    public abstract Task<Result<bool>> UserHasAccessAsync(User user, CancellationToken cancellationToken);
    public abstract Task<Result<string>> RemoveUserAsync(User user, CancellationToken cancellationToken);
    public abstract Task<Result<IList<UserStatus>>> GetUsersAsync(CancellationToken cancellationToken);
}
