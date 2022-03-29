using System.Net;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Services.Sharepoint;
using BcGov.Jag.AccountManagement.Shared;
using Refit;
using Serilog;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class UserManagementService : IUserManagementService
{
    private readonly ILogger<UserManagementService> _logger;

    private readonly IODataClientFactory _oDataClientFactory;
    private readonly ProjectConfigurationCollection _projects;
    private readonly ISamlAuthenticator _samlAuthenticator;
    private readonly IUserSearchService _userSearchService;


    public UserManagementService(
        ProjectConfigurationCollection projects,
        ILogger<UserManagementService> logger,
        IODataClientFactory oDataClientFactory,
        IUserSearchService userSearchService,
        ISamlAuthenticator samlAuthenticator)
    {
        _projects = projects ?? throw new ArgumentNullException(nameof(projects));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _oDataClientFactory = oDataClientFactory ?? throw new ArgumentNullException(nameof(oDataClientFactory));
        _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
        _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
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

    public async Task<List<ProjectResourceStatus>> AddUserToProjectAsync(User user, ProjectConfiguration project, CancellationToken cancellationToken)
    {
        var requests = CreateAddUserRequests(user, project, cancellationToken);

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
                string? message = task.Result;
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

    public async Task<List<Project>> GetProjectsForUserAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(user));
        }

        using var usernameScope = _logger.BeginScope(new Dictionary<string, object> { { "Username", user.UserName } });

        List<ProjectResourceAccess> requests = CreateUserHasAccessRequests();

        var tasks = requests.Select(request => request.Service.UserHasAccessAsync(user, cancellationToken)).ToList();
        await Task.WhenAll(tasks);
        //await Parallel.ForEachAsync(requests, async (request, cancellationToken) =>
        //{
        //    request.Access = await request.Service.UserHasAccessAsync(user, cancellationToken);
        //});

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



            //if (task.IsCompletedSuccessfully)
            //{
            //}
            //else if (task.IsFaulted)
            //{
            //    Exception exception = task.Exception?.InnerException;
            //    Guid errorId = Guid.NewGuid();

            //    string message = GetUserErrorMessageFor(exception, errorId);

            //    if (exception != null)
            //    {
            //        // log with exception
            //        _logger.LogError(exception, "Request to check {User} access to access to {Project} for {Resource} failed (Error ID: {ErrorId})",
            //            new { user.Id, user.UserName, user.UserPrincipalName },
            //            new { request.Configuration.Name, request.Configuration.Id },
            //            new { request.Resource.Type, request.Resource.Resource },
            //            errorId);
            //    }
            //    else
            //    {
            //        // log without exception
            //        _logger.LogError("Request to check {User} access to access to {Project} for {Resource} failed (Error ID: {ErrorId})",
            //            new { user.Id, user.UserName, user.UserPrincipalName },
            //            new { request.Configuration.Name, request.Configuration.Id },
            //            new { request.Resource.Type, request.Resource.Resource },
            //            errorId);
            //    }

            //    project.Resources.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Error, Message = message });
            //}

        }

        return projects;
    }

    public async Task<IList<UserStatus>> GetUsersAsync(ProjectConfiguration project, ProjectResource resource, CancellationToken cancellationToken)
    {
        var service = GetResourceUserManagementService(project, resource);

        var users = await service.GetUsersAsync(cancellationToken);

        return users;
    }

    private string GetUserErrorMessageFor(Exception exception, Guid errorId)
    {
        if (exception is ApiException apiException)
        {
            switch (apiException.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    return $"Application does not have permissions to check user's membership (Error Id: {errorId})";
                default:
                    return $"Remote project returned {apiException.StatusCode} when checking user's membership (Error Id: {errorId})";
            }
        }


        if (exception is TaskCanceledException taskCancelledException)
        {
            return $"Timeout checking user's membership (Error Id: {errorId})";
        }

        return $"Unknown error executing request id (Error Id: {errorId})";
    }

    public async Task<List<ProjectResourceStatus>> RemoveUserFromProjectAsync(User user, ProjectConfiguration project, CancellationToken cancellationToken)
    {
        var requests = CreateRemoveUserRequests(user, project, cancellationToken);

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
                string? message = task.Result;
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

    private List<ProjectResourceAccess> CreateUserHasAccessRequests()
    {
        List<ProjectResourceAccess> requests = new List<ProjectResourceAccess>();

        foreach (var project in _projects)
        {
            foreach (ProjectResource resource in project.Resources)
            {
                var service = GetResourceUserManagementService(project, resource);
                requests.Add(new ProjectResourceAccess(project, resource, service));
            }
        }

        return requests;
    }


    private List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<string> Task)> CreateAddUserRequests(User user, ProjectConfiguration project, CancellationToken cancellationToken)
    {
        List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<string> Task)> requests
            = new List<(ProjectConfiguration, ProjectResource, Task<string> Task)>();

        // this shouldn't happen
        if (user is null || string.IsNullOrEmpty(user.UserName)) return requests;

        foreach (var projectConfiguration in _projects.Where(_ => _.Name == project.Name))
        {
            foreach (ProjectResource resource in projectConfiguration.Resources)
            {
                var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                if (resourceUserManagementService != null)
                {
                    var task = Task.Run(() => resourceUserManagementService.AddUserAsync(user, cancellationToken), cancellationToken);
                    requests.Add((projectConfiguration, resource, task));
                }
            }
        }

        return requests;
    }

    private List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<string> Task)> CreateRemoveUserRequests(User user, ProjectConfiguration project, CancellationToken cancellationToken)
    {
        List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<string> Task)> requests = new List<(ProjectConfiguration, ProjectResource, Task<string>)>();

        foreach (var projectConfiguration in _projects.Where(_ => _.Name == project.Name))
        {
            foreach (ProjectResource resource in projectConfiguration.Resources)
            {
                var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                if (resourceUserManagementService != null)
                {
                    var task = Task.Run(() => resourceUserManagementService.RemoveUserAsync(user, cancellationToken), cancellationToken);
                    requests.Add((projectConfiguration, resource, task));
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
                return new DynamicsResourceUserManagementService(project, resource, _oDataClientFactory, _userSearchService, Log.Logger.ForContext<DynamicsResourceUserManagementService>());
            case ProjectType.SharePoint:
                return new SharePointResourceUserManagementService(project, resource, _userSearchService, _samlAuthenticator, Log.Logger.ForContext<SharePointResourceUserManagementService>());
            default:
                _logger.LogWarning("Unknown resource type {Type}, project resource will be skipped", resource.Type);
                return null;
        }
    }

    private class ProjectResourceAccess : ProjectResourceRequest<bool?>
    {
        public ProjectResourceAccess(ProjectConfiguration configuration, ProjectResource resource, IResourceUserManagementService service)
            : base(configuration, resource, service, null)
        {
        }

        public bool? Access
        {
            get { return State; }
            set { State = value; }
        }
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
