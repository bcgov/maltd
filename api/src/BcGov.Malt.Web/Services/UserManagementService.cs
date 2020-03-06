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
        private readonly ProjectConfigurationCollection _projects;
        private readonly ILogger<UserManagementService> _logger;
        private readonly IODataClientFactory _oDataClientFactory;

        public UserManagementService(ProjectConfigurationCollection projects, ILogger<UserManagementService> logger, IODataClientFactory oDataClientFactory)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _oDataClientFactory = oDataClientFactory ?? throw new ArgumentNullException(nameof(oDataClientFactory));
        }

        public Task<bool> AddUserToProjectAsync(User user, Project project)
        {
            _logger.LogError("Add user to project is not implemented, returning false");

            return Task.FromResult(false);
        }

        public async Task<List<Project>> GetProjectsForUserAsync(User user)
        {
            var requests = CreateRequests(user);

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

        private List<(ProjectConfiguration Configuration, ProjectResource Resource, Task<bool> Task)> CreateRequests(User user)
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
                    else
                    {
                        // invalid resource type
                    }
                }
            }

            return requests;
        }

        public Task<bool> RemoveUserFromProjectAsync(User user, Project project)
        {
            _logger.LogError("Remover user from project is not implemented, returning false");
            return Task.FromResult(false);
        }

        private IResourceUserManagementService GetResourceUserManagementService(ProjectConfiguration project, ProjectResource resource)
        {
            switch (resource.Type)
            {
                case ProjectType.Dynamics: 
                    return new DynamicsResourceUserManagementService(_oDataClientFactory, project, resource);
                case ProjectType.SharePoint: 
                    return new SharepointResourceUserManagementService(project, resource);
                default:
                    _logger.LogWarning("Unknown resource type {Type}, project resource will be skipped", resource.Type);
                    return null;
            }
        }
    }
}
