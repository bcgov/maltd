using System;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
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
        /// <summary>
        /// Gets the list of available projects.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Project[]>> GetAsync()
        {
            await Task.Delay(0); // to suppress warning CS1998: This async method lacks 'await'
            return Array.Empty<Project>();
        }
    }
}
