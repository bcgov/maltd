using System.Collections.Generic;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BcGov.Malt.Web.Controllers
{
    /// <summary>
    /// </summary>
    [Route("api/projects")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        /// <summary>Initializes a new instance of the <see cref="ProjectsController"/> class.</summary>
        /// <param name="projectService">The project service.</param>
        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService ?? throw new System.ArgumentNullException(nameof(projectService));
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
            var projects = await _projectService.GetProjectsAsync();
            return Ok(projects);
        }
    }
}
