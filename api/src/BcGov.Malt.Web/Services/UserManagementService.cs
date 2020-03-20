using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly ILogger<UserManagementService> _logger;

        private readonly IODataClientFactory _oDataClientFactory;
        private readonly ProjectConfigurationCollection _projects;

        private readonly ILogger<SharePointResourceUserManagementService> _sharePointResourceUserManagementServiceLogger;
        private readonly IUserSearchService _userSearchService;
        private readonly ILogger<DynamicsResourceUserManagementService> _dynamicsResourceUserManagementService;


        public UserManagementService(
            ProjectConfigurationCollection projects, 
            ILogger<UserManagementService> logger, 
            IODataClientFactory oDataClientFactory, 
            IUserSearchService userSearchService,
            ILogger<DynamicsResourceUserManagementService> dynamicsResourceUserManagementService,
            ILogger<SharePointResourceUserManagementService> sharePointResourceUserManagementServiceLogger)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _oDataClientFactory = oDataClientFactory ?? throw new ArgumentNullException(nameof(oDataClientFactory));
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
            _dynamicsResourceUserManagementService = dynamicsResourceUserManagementService ?? throw new ArgumentNullException(nameof(dynamicsResourceUserManagementService));
            _sharePointResourceUserManagementServiceLogger = sharePointResourceUserManagementServiceLogger ?? throw new ArgumentNullException(nameof(sharePointResourceUserManagementServiceLogger));
        }

        public async Task<bool> AddUserToProjectAsync(User user, ProjectConfiguration project)
        {
            var requests = CreateAddUserRequests(user, project);

            // wait for all tasks to complete
            Task aggregateTask = Task.WhenAll(requests.Select(_ => _.Task));

            try
            {
                await aggregateTask;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Error on aggregate request");
            }

            // TODO: do we need to return true/false based on result?
            return true;
        }

        public async Task<List<Project>> GetProjectsForUserAsync(User user)
        {
            var requests = CreateUserHasAccessRequests(user);

            // wait for all tasks to complete
            Task<bool[]> aggregateTask = Task.WhenAll(requests.Select(_ => _.Task));

            try
            {
                await aggregateTask;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Error on aggregate request");
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

                if (request.Task.IsCompletedSuccessfully)
                {
                    _logger.LogDebug("Request to {Project} for {Resource} completed successfully", request.Configuration.Name, request.Resource.Type);
                    bool userHasAccess = request.Task.Result;
                    project.Resources.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = userHasAccess ? "member" : "not-member" });

                }
                else if (request.Task.IsFaulted)
                {
                    if (request.Task.Exception != null)
                    {
                        _logger.LogError(request.Task.Exception, "Request to {Project} for {Resource} failed", request.Configuration.Name, request.Resource.Type);
                    }
                    else
                    {
                        _logger.LogError("Request to {Project} for {Resource} failed", request.Configuration.Name, request.Resource.Type);
                    }

                    project.Resources.Add(new ProjectResourceStatus { Type = request.Resource.Type.ToString(), Status = "error" });
                }
            }

            return projects;
        }

        public async Task<bool> RemoveUserFromProjectAsync(User user, ProjectConfiguration project)
        {
            var requests = CreateRemoveUserRequests(user, project);

            // wait for all tasks to complete
            Task aggregateTask = Task.WhenAll(requests.Select(_ => _.Task));

            try
            {
                await aggregateTask;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Error on aggregate request");
            }

            // TODO: do we need to return true/false based on result?
            return true;
        }

        private List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<bool> Task)> CreateUserHasAccessRequests(User user)
        {
            List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<bool> Task)> requests = new List<(ProjectConfiguration, ProjectResource, Task<bool>)>();

            foreach (var projectConfiguration in _projects)
            {
                foreach (ProjectResource resource in projectConfiguration.Resources)
                {
                    var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                    if (resourceUserManagementService != null)
                    {
                        var task = resourceUserManagementService.UserHasAccessAsync(user.UserName);
                        requests.Add((projectConfiguration, resource, task));
                    }
                }
            }

            return requests;
        }

        private List<(ProjectConfiguration Configuration, ProjectResource Resource, Task Task)> CreateAddUserRequests(User user, ProjectConfiguration project)
        {
            List<(ProjectConfiguration Configuration, ProjectResource Resource, Task Task)> requests = new List<(ProjectConfiguration, ProjectResource, Task)>();

            foreach (var projectConfiguration in _projects.Where(_ => _.Id == project.Id))
            {
                foreach (ProjectResource resource in projectConfiguration.Resources)
                {
                    var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                    if (resourceUserManagementService != null)
                    {
                        var task = resourceUserManagementService.AddUserAsync(user.UserName);
                        requests.Add((projectConfiguration, resource, task));
                    }
                }
            }

            return requests;
        }


        private List<(ProjectConfiguration Configuration, ProjectResource Resource, Task Task)> CreateRemoveUserRequests(User user, ProjectConfiguration project)
        {
            List<(ProjectConfiguration Configuration, ProjectResource Resource, Task Task)> requests = new List<(ProjectConfiguration, ProjectResource, Task)>();

            foreach (var projectConfiguration in _projects.Where(_ => _.Id == project.Id))
            {
                foreach (ProjectResource resource in projectConfiguration.Resources)
                {
                    var resourceUserManagementService = GetResourceUserManagementService(projectConfiguration, resource);
                    if (resourceUserManagementService != null)
                    {
                        var task = resourceUserManagementService.RemoveUserAsync(user.UserName);
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
                    return new SharePointResourceUserManagementService(project, resource, _userSearchService, _sharePointResourceUserManagementServiceLogger);
                default:
                    _logger.LogWarning("Unknown resource type {Type}, project resource will be skipped", resource.Type);
                    return null;
            }
        }
    }
}
