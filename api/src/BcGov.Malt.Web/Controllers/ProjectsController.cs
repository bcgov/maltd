using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BcGov.Malt.Web.Features.Projects;
using BcGov.Malt.Web.Models;
using MediatR;
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
        private readonly IMediator _mediator;
        private readonly ILogger<ProjectsController> _logger;

        /// <summary>Initializes a new instance of the <see cref="ProjectsController"/> class.</summary>
        /// <param name="mediator"></param>
        /// <param name="logger">The logger.</param>
        public ProjectsController(IMediator mediator, ILogger<ProjectsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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

            var projects = await _mediator.Send(new ListProjects.Request());

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
            if (string.IsNullOrEmpty(project))
            {
                _logger.LogInformation("Required parameter {Parameter} was not specified, returning 400 Bad Request", nameof(project));
                return BadRequest();
            }

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogInformation("Required parameter {Parameter} was not specified, returning 400 Bad Request", nameof(username));
                return BadRequest();
            }

            _logger.LogDebug("Adding {username} to {project}", username, project);

            bool added = await _mediator.Send(new AddUserToProject.Request(project, username));

            return Ok();
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
            if (string.IsNullOrEmpty(project))
            {
                _logger.LogInformation("Required parameter {Parameter} was not specified, returning 400 Bad Request", nameof(project));
                return BadRequest();
            }

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogInformation("Required parameter {Parameter} was not specified, returning 400 Bad Request", nameof(username));
                return BadRequest();
            }

            _logger.LogDebug("Removing {username} from {project}", username, project);

            bool removed = await _mediator.Send(new RemoveUserFromProject.Request(project, username));

            return Ok();
        }
    }
}
