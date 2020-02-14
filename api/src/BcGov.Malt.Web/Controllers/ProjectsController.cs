using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace BcGov.Malt.Web.Controllers
{
    /// <summary>
    /// </summary>
    [Route("api/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IUserSearchService _userSearchService;
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<ProjectsController> _logger;

        /// <summary>Initializes a new instance of the <see cref="ProjectsController"/> class.</summary>
        /// <param name="projectService">The project service.</param>
        /// <param name="userSearchService">The user search service</param>
        /// <param name="userManagementService">The user managment service.</param>
        /// <param name="logger">The logger.</param>
        public ProjectsController(
            IProjectService projectService,
            IUserSearchService userSearchService,
            IUserManagementService userManagementService,
            ILogger<ProjectsController> logger)
        {
            _projectService = projectService ?? throw new System.ArgumentNullException(nameof(projectService));
            _userSearchService = userSearchService ?? throw new System.ArgumentNullException(nameof(userSearchService));
            _userManagementService = userManagementService ?? throw new System.ArgumentNullException(nameof(userManagementService));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the list of available projects.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Project>>> GetAsync()
        {
            _logger.LogDebug("Getting list of available projects");

            var projects = await _projectService.GetProjectsAsync();
            return Ok(projects);
        }


        /// <summary>Adds a user to a project.</summary>
        /// <param name="username">The username of the user</param>
        /// <param name="project">The project identifier.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{project}/users/{username}")]
        [SwaggerOperation(OperationId = "AddUserToProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> AddUserToProjectAsync(string project, string username)
        {
            if (project == null)
            {
                _logger.LogInformation("Required parameter {parameter} was not specified, returning 400 Bad Request", nameof(project));
                return BadRequest();
            }

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogInformation("Required parameter {parameter} was not specified, returning 400 Bad Request", nameof(username));
                return BadRequest();
            }

            _logger.LogDebug("Adding {username} to {project}", username, project);

            ActionResult result = await AddOrRemoveUserFromProject(project, username, _userManagementService.AddUserToProjectAsync);
            return result;
        }

        /// <summary>Removes a user from a project.</summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="project">The project identifier.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{project}/users/{username}")]
        [SwaggerOperation(OperationId = "RemoveUserFromProject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RemoveUserFromProjectAsync(string project, string username)
        {
            if (project == null)
            {
                _logger.LogInformation("Required parameter {parameter} was not specified, returning 400 Bad Request", nameof(project));
                return BadRequest();
            }

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogInformation("Required parameter {parameter} was not specified, returning 400 Bad Request", nameof(username));
                return BadRequest();
            }

            _logger.LogDebug("Removing {username} from {project}", username, project);

            ActionResult result = await AddOrRemoveUserFromProject(project, username, _userManagementService.RemoveUserFromProjectAsync);
            return result;
        }

        /// <summary>
        /// Adds or removes a user from project.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="username">The username.</param>
        /// <param name="operation">The add/remove operation.</param>
        /// <returns></returns>
        private async Task<ActionResult> AddOrRemoveUserFromProject(string project, string username, Func<User, Project, Task<bool>> operation)
        {
            Debug.Assert(project != null);
            Debug.Assert(username != null);
            Debug.Assert(operation != null);

            List<Project> projects = await _projectService.GetProjectsAsync();

            Project projectObject = projects.SingleOrDefault(_ => _.Id == project);

            if (projectObject == null)
            {
                // TODO: handle difference between project not found vs user not found
                return NotFound();
            }

            User user = await _userSearchService.SearchAsync(username);

            if (user == null)
            {
                // TODO: handle difference between project not found vs user not found
                return NotFound();
            }

            bool success = await operation(user, projectObject);

            if (success)
            {
                return Ok();
            }

            // TODO: determine correct return status / data when the user could not be added to the project
            return Ok();
        }
    }
}
