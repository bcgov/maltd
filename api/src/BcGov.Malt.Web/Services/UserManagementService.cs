using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Services.Sharepoint;
using Microsoft.Extensions.Logging;
using Refit;

namespace BcGov.Malt.Web.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ILogger<UserManagementService> _logger;

        private readonly IODataClientFactory _oDataClientFactory;
        private readonly ProjectConfigurationCollection _projects;

        private readonly ILogger<SharePointResourceUserManagementService> _sharePointResourceUserManagementServiceLogger;
        private readonly ISamlAuthenticator _samlAuthenticator;
        private readonly IUserSearchService _userSearchService;
        private readonly ILogger<DynamicsResourceUserManagementService> _dynamicsResourceUserManagementService;


        public UserManagementService(
            ProjectConfigurationCollection projects,
            ILogger<UserManagementService> logger,
            IODataClientFactory oDataClientFactory,
            IUserSearchService userSearchService,
            ILogger<DynamicsResourceUserManagementService> dynamicsResourceUserManagementService,
            ILogger<SharePointResourceUserManagementService> sharePointResourceUserManagementServiceLogger,
            ISamlAuthenticator samlAuthenticator)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _oDataClientFactory = oDataClientFactory ?? throw new ArgumentNullException(nameof(oDataClientFactory));
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
            _dynamicsResourceUserManagementService = dynamicsResourceUserManagementService ?? throw new ArgumentNullException(nameof(dynamicsResourceUserManagementService));
            _sharePointResourceUserManagementServiceLogger = sharePointResourceUserManagementServiceLogger ?? throw new ArgumentNullException(nameof(sharePointResourceUserManagementServiceLogger));
            _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
        }

        public async Task<List<ProjectResourceStatus>> AddUserToProjectAsync(User user, ProjectConfiguration project, CancellationToken cancellationToken)
        {
            var requests = CreateAddUserRequests(user, project, cancellationToken);

            // wait for all tasks to complete
            Task aggregateTask = Task.WhenAll(requests.Select(_ => _.Task));

            try
            {
                await aggregateTask.ConfigureAwait(false);
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
                    string message = task.Result;
                    if (string.IsNullOrEmpty(message))
                    {
                        message = null;
                    }

                    _logger.LogDebug("Request to add {User} to {Project} {Resource} completed successfully",
                        new { user.UserName, user.Email },
                        new { request.Configuration.Name, request.Configuration.Id },
                        new { request.Resource.Type, request.Resource.Resource });

                    statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Member, Message = message });
                }
                else if (task.IsFaulted)
                {
                    Guid requestId = Guid.NewGuid();
                    string message = $"Unknown error executing request id {requestId}";

                    if (task.Exception != null)
                    {
                        // log with exception
                        _logger.LogError(task.Exception,
                            "Request add user {@User} to project {Project} for resource {Resource} failed ({RequestId}",
                            new { user.Id, user.UserName, user.UserPrincipalName },
                            new { request.Configuration.Name, request.Configuration.Id },
                            new { request.Resource.Type, request.Resource.Resource },
                            requestId);
                    }
                    else
                    {
                        // log without exception
                        _logger.LogError("Request add user {@User} to project {Project} for resource {Resource} failed ({RequestId}",
                            new { user.Id, user.UserName, user.UserPrincipalName },
                            new { request.Configuration.Name, request.Configuration.Id },
                            new { request.Resource.Type, request.Resource.Resource },
                            requestId);
                    }

                    statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Error, Message = message });
                }
            }

            return statuses;
        }

        public async Task<List<Project>> GetProjectsForUserAsync(User user, CancellationToken cancellationToken)
        {
            var requests = CreateUserHasAccessRequests(user, cancellationToken);

            // wait for all tasks to complete
            Task<bool[]> aggregateTask = Task.WhenAll(requests.Select(_ => _.Task));

            try
            {
                await aggregateTask.ConfigureAwait(false);
            }
            catch (Exception)
            {
                // if any of the tasks throws an error, that first exception will be thrown
                // we can safely ignore this exception because we are going to iterate over
                // all of the requests and if faulted, log them out too
            }

            List<Project> projects = new List<Project>();

            foreach (var request in requests)
            {
                Project project = projects.SingleOrDefault(_ => _.Id == request.Configuration.Id);
                if (project == null)
                {
                    project = new Project
                    {
                        Id = request.Configuration.Id,
                        Name = request.Configuration.Name,
                        Resources = new List<ProjectResourceStatus>()
                    };

                    projects.Add(project);
                }

                var task = request.Task;

                if (task.IsCompletedSuccessfully)
                {
                    _logger.LogDebug("Request to {Project} for {Resource} completed successfully",
                        new { request.Configuration.Name, request.Configuration.Id },
                        new { request.Resource.Type, request.Resource.Resource });

                    bool userHasAccess = task.Result;
                    project.Resources.Add(new ProjectResourceStatus
                    {
                        Type = request.Resource.Type.ToString(),
                        Status = userHasAccess
                            ? ProjectResourceStatuses.Member
                            : ProjectResourceStatuses.NotMember
                    });

                }
                else if (task.IsFaulted)
                {
                    Exception exception = task.Exception?.InnerException;
                    Guid requestId = Guid.NewGuid();

                    string message = GetUserErrorMessageFor(exception, requestId);
                    
                    if (exception != null)
                    {
                        // log with exception
                        _logger.LogError(exception, "Request to get user {User} access status on project {Project} for resource {Resource} failed ({RequestId}",
                            new { user.Id, user.UserName, user.UserPrincipalName },
                            new { request.Configuration.Name, request.Configuration.Id },
                            new { request.Resource.Type, request.Resource.Resource },
                            requestId);
                    }
                    else
                    {
                        // log without exception
                        _logger.LogError("Request to get user {User} access status on project {Project} for resource {Resource} failed ({RequestId}",
                            new { user.Id, user.UserName, user.UserPrincipalName },
                            new { request.Configuration.Name, request.Configuration.Id },
                            new { request.Resource.Type, request.Resource.Resource },
                            requestId);
                    }

                    project.Resources.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Error, Message = message });
                }
            }

            return projects;
        }

        private string GetUserErrorMessageFor(Exception exception, Guid requestId)
        {
            if (exception is ApiException apiException)
            {
                switch (apiException.StatusCode)
                {
                    case HttpStatusCode.Forbidden:
                        return $"Application does not have permissions to check user's membership (Error Id: {requestId})";
                    default:
                        return $"Remote project returned {apiException.StatusCode} when checking user's membership (Error Id: {requestId})";
                }
            }


            if (exception is TaskCanceledException taskCancelledException)
            {
                return $"Timeout checking user's membership (Error Id: {requestId})";
            }

            return $"Unknown error executing request id {requestId}";
        }

        public async Task<List<ProjectResourceStatus>> RemoveUserFromProjectAsync(User user, ProjectConfiguration project, CancellationToken cancellationToken)
        {
            var requests = CreateRemoveUserRequests(user, project, cancellationToken);

            // wait for all tasks to complete
            Task aggregateTask = Task.WhenAll(requests.Select(_ => _.Task));

            try
            {
                await aggregateTask.ConfigureAwait(false);
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
                    string message = task.Result;
                    if (string.IsNullOrEmpty(message))
                    {
                        message = null;
                    }

                    _logger.LogDebug("Request to remove {User} from {Project} {Resource} completed successfully",
                        new { user.UserName, user.Email },
                        new { request.Configuration.Name, request.Configuration.Id },
                        new { request.Resource.Type, request.Resource.Resource });

                    statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.NotMember, Message = message });
                }
                else if (task.IsFaulted)
                {
                    Guid requestId = Guid.NewGuid();
                    string message = $"Unknown error executing request id {requestId}";

                    if (task.Exception != null)
                    {
                        // log with exception
                        _logger.LogError(task.Exception,
                            "Request to remove user {@User} from project {Project} for resource {Resource} failed ({RequestId}",
                            new { user.Id, user.UserName, user.UserPrincipalName },
                            new { request.Configuration.Name, request.Configuration.Id },
                            new { request.Resource.Type, request.Resource.Resource },
                            requestId);
                    }
                    else
                    {
                        // log without exception
                        _logger.LogError("Request to remove user {@User} fromproject {Project} for resource {Resource} failed ({RequestId}",
                            new { user.Id, user.UserName, user.UserPrincipalName },
                            new { request.Configuration.Name, request.Configuration.Id },
                            new { request.Resource.Type, request.Resource.Resource },
                            requestId);
                    }

                    statuses.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = ProjectResourceStatuses.Error, Message = message });
                }
            }

            return statuses;
        }

        private List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<bool> Task)> CreateUserHasAccessRequests(User user, CancellationToken cancellationToken)
        {
            List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<bool> Task)> requests = new List<(ProjectConfiguration, ProjectResource, Task<bool>)>();

            foreach (var projectConfiguration in _projects)
            {
                foreach (ProjectResource resource in projectConfiguration.Resources)
                {
                    var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                    if (resourceUserManagementService != null)
                    {
                        var task = resourceUserManagementService.UserHasAccessAsync(user.UserName, cancellationToken);
                        requests.Add((projectConfiguration, resource, task));
                    }
                }
            }

            return requests;
        }

        private List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<string> Task)> CreateAddUserRequests(User user, ProjectConfiguration project, CancellationToken cancellationToken)
        {
            List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<string> Task)> requests
                = new List<(ProjectConfiguration, ProjectResource, Task<string> Task)>();

            foreach (var projectConfiguration in _projects.Where(_ => _.Id == project.Id))
            {
                foreach (ProjectResource resource in projectConfiguration.Resources)
                {
                    var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                    if (resourceUserManagementService != null)
                    {
                        var task = resourceUserManagementService.AddUserAsync(user.UserName, cancellationToken);
                        requests.Add((projectConfiguration, resource, task));
                    }
                }
            }

            return requests;
        }

        private List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<string> Task)> CreateRemoveUserRequests(User user, ProjectConfiguration project, CancellationToken cancellationToken)
        {
            List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<string> Task)> requests = new List<(ProjectConfiguration, ProjectResource, Task<string>)>();

            foreach (var projectConfiguration in _projects.Where(_ => _.Id == project.Id))
            {
                foreach (ProjectResource resource in projectConfiguration.Resources)
                {
                    var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                    if (resourceUserManagementService != null)
                    {
                        var task = resourceUserManagementService.RemoveUserAsync(user.UserName, cancellationToken);
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
                    return new DynamicsResourceUserManagementService(project, resource, _oDataClientFactory, _userSearchService, _dynamicsResourceUserManagementService);
                case ProjectType.SharePoint:
                    return new SharePointResourceUserManagementService(project, resource, _userSearchService, _samlAuthenticator, _sharePointResourceUserManagementServiceLogger);
                default:
                    _logger.LogWarning("Unknown resource type {Type}, project resource will be skipped", resource.Type);
                    return null;
            }
        }
    }
}
