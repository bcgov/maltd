using BcGov.Jag.AccountManagement.Server.Infrastructure;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Services.Sharepoint;
using BcGov.Jag.AccountManagement.Shared;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IProjectConfigurationRepository _projectConfigurationRepository;
    private readonly ILogger<UserManagementService> _logger;
    private readonly IODataClientFactory _oDataClientFactory;
    private readonly ISamlAuthenticator _samlAuthenticator;
    private readonly IServiceProvider _serviceProvider;

    public UserManagementService(
        IProjectConfigurationRepository projectConfigurationRepository,
        ILogger<UserManagementService> logger,
        IODataClientFactory oDataClientFactory,
        ISamlAuthenticator samlAuthenticator,
        IServiceProvider serviceProvider
        )
    {
        _projectConfigurationRepository = projectConfigurationRepository ?? throw new ArgumentNullException(nameof(projectConfigurationRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _oDataClientFactory = oDataClientFactory ?? throw new ArgumentNullException(nameof(oDataClientFactory));
        _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task ChangeUserProjectAccessAsync(
        User user, 
        ProjectConfiguration project,
        ProjectMembershipModel projectMembership,
        CancellationToken cancellationToken)
    {
        if (projectMembership.Dynamics.HasValue)
        {
            await AddRemoveUserAsync(user, project, ProjectType.Dynamics, projectMembership.Dynamics.Value, cancellationToken);
        }

        if (projectMembership.SharePoint.HasValue)
        {
            await AddRemoveUserAsync(user, project, ProjectType.SharePoint, projectMembership.SharePoint.Value, cancellationToken);
        }
    }

    private async Task AddRemoveUserAsync(User user, ProjectConfiguration project, ProjectType projectType, bool add, CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Change User Project Access");
        activity?.AddTag("operation", add ? "Add" : "Remove");

        ProjectResource? resource = project.Resources.SingleOrDefault(_ => _.Type == projectType);
        if (resource is null)
        {
            _logger.LogWarning("Requested to add/remove user from {Resource} but {Project} is not configured with this resource.", projectType, project.Name);
        }
        else
        {
            IResourceUserManagementService service = GetResourceUserManagementService(project, resource);
            if (add)
            {
                _logger.LogInformation("Adding {Username} to {Project} - {Resource}", user.UserName, project.Name, projectType);
                await service.AddUserAsync(user, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Removing {Username} from {Project} - {Resource}", user.UserName, project.Name, projectType);
                await service.RemoveUserAsync(user, cancellationToken);
            }
        }
    }

    public async Task<Result<List<ProjectResourceStatus>>> AddUserToProjectAsync(User user, ProjectConfiguration project, CancellationToken cancellationToken)
    {
        var requests = await CreateAddUserRequests(user, project, cancellationToken);

        // wait for all tasks to complete
        Task aggregateTask = Task.WhenAll(requests.Select(_ => Task.Run(() => _.Task, cancellationToken)));

        try
        {
            await aggregateTask;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Error on aggregate request");
        }

        var statuses = new List<ProjectResourceStatus>();

        foreach (var request in requests)
        {
            var task = request.Task;

            if (task.IsCompletedSuccessfully)
            {
                var result = task.Result;
                if (result.IsSuccess)
                {
                    string? message = result.Value;
                    if (string.IsNullOrEmpty(message))
                    {
                        message = null;
                    }

                    _logger.LogDebug("Request to add {User} to {Project} {Resource} completed successfully",
                        new { user.UserName, user.Email },
                        new { request.Configuration.Name },
                        new { request.Resource.Type, request.Resource.Resource });

                    statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Member, Message = message });
                }
                else
                {
                    var error = result.Errors[0];
                    statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Error, Message = error.Message });
                }
            }
            else if (task.IsFaulted)
            {
                Guid errorId = Guid.NewGuid();
                string message = $"Unknown error executing request, error id: {errorId}";

                if (task.Exception != null)
                {
                    // log with exception
                    _logger.LogError(task.Exception,
                        "Request add user {@User} to project {Project} for resource {Resource} failed (Error Id: {ErrorId})",
                        new { user.Id, user.UserName, user.UserPrincipalName },
                        new { request.Configuration.Name },
                        new { request.Resource.Type, request.Resource.Resource },
                        errorId);
                }
                else
                {
                    // log without exception
                    _logger.LogError("Request add user {@User} to project {Project} for resource {Resource} failed (Error Id: {ErrorId})",
                        new { user.Id, user.UserName, user.UserPrincipalName },
                        new { request.Configuration.Name },
                        new { request.Resource.Type, request.Resource.Resource },
                        errorId);
                }

                statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Error, Message = message });
            }
        }

        return statuses;
    }

    public async Task<Result<List<Project>>> GetProjectsForUserAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(user));
        }

        using var activity = Diagnostics.Source.StartActivity("Get Projects For User");

        using var usernameScope = _logger.BeginScope(new Dictionary<string, object> { { "Username", user.UserName } });

        // TODO: use supplied projects to build request pipeline
        List<ProjectResourceAccess> requests = await CreateUserHasAccessRequests(cancellationToken);

        var tasks = requests.Select(request => request.CheckUserHasAccessAsync(user, cancellationToken)).ToList();
        await Task.WhenAll(tasks);

        List<Project> projects = new List<Project>();

        foreach (var requestGroup in requests.GroupBy(_ => _.Configuration))
        {
            Project project = new Project
            {
                Name = requestGroup.Key.Name,
                Resources = new List<ProjectResourceStatus>()
            };

            projects.Add(project);

            foreach (var requestItem in requestGroup)
            {
                if (requestItem.Errors.Count != 0)
                {
                    project.Resources.Add(new ProjectResourceStatus
                    {
                        Type = requestItem.Resource.Type.ToString(),
                        Status = ProjectResourceStatuses.Error,
                        Message = requestItem.Errors[0].Message
                    });
                }
                else
                {
                    // TODO: handle null
                    bool userHasAccess = requestItem.Access is not null && requestItem.Access.Value;

                    project.Resources.Add(new ProjectResourceStatus
                    {
                        Type = requestItem.Resource.Type.ToString(),
                        Status = userHasAccess
                            ? ProjectResourceStatuses.Member
                            : ProjectResourceStatuses.NotMember
                    });
                }
            }
        }

        return projects;
    }

    public async Task<Result<IList<UserStatus>>> GetUsersAsync(ProjectConfiguration project, ProjectResource resource, CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Get Project Users");
        activity?.AddTag("project.name", project.Name);

        var service = GetResourceUserManagementService(project, resource);

        var users = await service.GetUsersAsync(cancellationToken);

        return users;
    }

    public async Task<Result<List<ProjectResourceStatus>>> RemoveUserFromProjectAsync(User user, ProjectConfiguration project, CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Remove User From Project");
        activity?.AddTag("project.name", project.Name);

        var requests = await CreateRemoveUserRequests(user, project, cancellationToken);

        // wait for all tasks to complete
        Task aggregateTask = Task.WhenAll(requests.Select(_ => Task.Run(() => _.Task, cancellationToken)));

        try
        {
            await aggregateTask;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Error on aggregate request");
        }

        var statuses = new List<ProjectResourceStatus>();

        foreach (var request in requests)
        {
            var task = request.Task;

            if (task.IsCompletedSuccessfully)
            {
                var result = task.Result;
                if (result.IsSuccess)
                {
                    string? message = result.Value;
                    if (string.IsNullOrEmpty(message))
                    {
                        message = null;
                    }

                    _logger.LogDebug("Request to remove {User} from {Project} {Resource} completed successfully",
                        new { user.UserName, user.Email },
                        new { request.Configuration.Name },
                        new { request.Resource.Type, request.Resource.Resource });

                    statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.NotMember, Message = message });
                }
                else
                {
                    var error = result.Errors[0];
                    statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Error, Message = error.Message });
                }

            }
            else if (task.IsFaulted)
            {
                Guid errorId = Guid.NewGuid();
                string message = $"Unknown error executing request id {errorId}";

                if (task.Exception != null)
                {
                    // log with exception
                    _logger.LogError(task.Exception,
                        "Request to remove user {@User} from project {Project} for resource {Resource} failed (Error Id: {errorId})",
                    new { user.Id, user.UserName, user.UserPrincipalName },
                        new { request.Configuration.Name },
                        new { request.Resource.Type, request.Resource.Resource },
                        errorId);
                }
                else
                {
                    // log without exception
                    _logger.LogError("Request to remove user {@User} from project {Project} for resource {Resource} failed (Error Id: {errorId})",
                    new { user.Id, user.UserName, user.UserPrincipalName },
                        new { request.Configuration.Name },
                        new { request.Resource.Type, request.Resource.Resource },
                        errorId);
                }

                statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Error, Message = message });
            }
        }

        return statuses;
    }

    private async Task<List<ProjectResourceAccess>> CreateUserHasAccessRequests(CancellationToken cancellationToken)
    {
        var projects = await _projectConfigurationRepository.GetProjectConfigurationsAsync(cancellationToken);

        List<ProjectResourceAccess> requests = new List<ProjectResourceAccess>();

        foreach (var project in projects)
        {
            foreach (ProjectResource resource in project.Resources)
            {
                var service = GetResourceUserManagementService(project, resource);
                requests.Add(new ProjectResourceAccess(project, resource, service));
            }
        }

        return requests;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Configuration">The project configuration the request is for</param>
    /// <param name="Resource">The project configuration resource the request is for</param>
    /// <param name="Task"></param>
    record UserRequest<T>(ProjectConfiguration Configuration, ProjectResource Resource, Task<T> Task);

    private async Task<List<UserRequest<Result<string>>>> CreateAddUserRequests(User user, ProjectConfiguration project, CancellationToken cancellationToken)
    {
        List<UserRequest<Result<string>>> requests = new List<UserRequest<Result<string>>>();

        // this shouldn't happen
        if (user is null || string.IsNullOrEmpty(user.UserName)) return requests;

        var projects = await _projectConfigurationRepository.GetProjectConfigurationsAsync(cancellationToken);

        foreach (var projectConfiguration in projects.Where(_ => _.Name == project.Name))
        {
            foreach (ProjectResource resource in projectConfiguration.Resources)
            {
                var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                if (resourceUserManagementService != null)
                {
                    var task = Task.Run(() => resourceUserManagementService.AddUserAsync(user, cancellationToken), cancellationToken);
                    requests.Add(new UserRequest<Result<string>>(projectConfiguration, resource, task));
                }
            }
        }

        return requests;
    }

    private async Task<List<UserRequest<Result<string>>>> CreateRemoveUserRequests(User user, ProjectConfiguration project, CancellationToken cancellationToken)
    {
        List<UserRequest<Result<string>>> requests = new List<UserRequest<Result<string>>>();

        var projects = await _projectConfigurationRepository.GetProjectConfigurationsAsync(cancellationToken);

        foreach (var projectConfiguration in projects.Where(_ => _.Name == project.Name))
        {
            foreach (ProjectResource resource in projectConfiguration.Resources)
            {
                var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                if (resourceUserManagementService != null)
                {
                    var task = Task.Run(() => resourceUserManagementService.RemoveUserAsync(user, cancellationToken), cancellationToken);
                    requests.Add(new UserRequest<Result<string>>(projectConfiguration, resource, task));
                }
            }
        }

        return requests;
    }

    private IResourceUserManagementService GetResourceUserManagementService(ProjectConfiguration project, ProjectResource resource)
    {
        switch (resource.Type)
        {
            case ProjectType.Dynamics:
                return new DynamicsResourceUserManagementService(project, resource, _oDataClientFactory, Log.Logger.ForContext<DynamicsResourceUserManagementService>());
            case ProjectType.SharePoint:
                var cache = _serviceProvider.GetRequiredService<IMemoryCache>();
                return new SharePointResourceUserManagementService(project, resource, _samlAuthenticator, cache, Log.Logger.ForContext<SharePointResourceUserManagementService>());
            default:
                _logger.LogWarning("Unknown resource type {Type}, project resource will be skipped", resource.Type);
                return null!;
        }
    }

    private class ProjectResourceAccess : ProjectResourceRequest<bool?>
    {
        public ProjectResourceAccess(ProjectConfiguration configuration, ProjectResource resource, IResourceUserManagementService service)
            : base(configuration, resource, service, null)
        {
        }

        public async Task CheckUserHasAccessAsync(User user, CancellationToken cancellationToken)
        {
            var result = await Service.UserHasAccessAsync(user, cancellationToken);
            if (result.IsSuccess)
            {
                Access = result.Value;
            }
            else
            {
                Errors = result.Errors;
            }
        }

        public bool? Access
        {
            get { return State; }
            set { State = value; }
        }

        public IList<IError> Errors { get; private set; } = Array.Empty<IError>();
    }

    private abstract class ProjectResourceRequest<TState>
    {
        protected ProjectResourceRequest(ProjectConfiguration configuration, ProjectResource resource, IResourceUserManagementService service, TState stateDefault)
        {
            Configuration = configuration;
            Resource = resource;
            Service = service;
            State = stateDefault;
        }

        public ProjectConfiguration Configuration { get; init; }
        public ProjectResource Resource { get; init; }
        public IResourceUserManagementService Service { get; init; }

        protected TState State { get; set; }
    }
}
