using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace BcGov.Malt.Web.Controllers
{
    /// <summary>
    /// Provides access to users
    /// </summary>
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;

        private readonly IUserSearchService _userSearchService;
        private readonly IUserManagementService _userManagementService;

        /// <summary>Initializes a new instance of the <see cref="UsersController" /> class.</summary>
        /// <param name="userSearchService">The user search service.</param>
        /// <param name="userManagementService"></param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="userSearchService" />, <paramref name="userManagementService" /> 
        /// or <paramref name="logger" /> is null.
        /// </exception>
        public UsersController(IUserSearchService userSearchService, IUserManagementService userManagementService, ILogger<UsersController> logger)
        {
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
            _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Searches for a user.</summary>
        /// <param name="query">The username to search for.</param>
        /// <returns>The found user or 400, 401 or 404 status codes.</returns>
        [HttpGet]
        [SwaggerOperation(OperationId = "UserSearch")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> SearchAsync([FromQuery(Name = "q")] [BindRequired] string query)
        {
            // TODO: user model validation

            if (string.IsNullOrEmpty(query)) return BadRequest();

            var user = await _userSearchService.SearchAsync(query);

            if (user != null) return user;

            return NotFound();
        }

        /// <summary>Gets a user</summary>
        /// <param name="username">The username to get</param>
        /// <returns>The user details or 400, 401 or 404 status codes</returns>
        [HttpGet]
        [Route("{username}")]
        [SwaggerOperation(OperationId = "UserLookup")]
        [ProducesResponseType(typeof(DetailedUser), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DetailedUser>> LookupAsync(string username)
        {
            // TODO: user model validation

            if (string.IsNullOrEmpty(username)) return BadRequest();

            var user = await _userSearchService.SearchAsync(username);

            if (user != null)
            {
                var projects = await _userManagementService.GetProjectsForUserAsync(user);

                DetailedUser result = CreateDetailedUser(user, projects);

                return result;
            }

            return NotFound();
        }

        /// <summary>Adds the user to a project.</summary>
        /// <param name="username">The username.</param>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{username}")]
        [SwaggerOperation(OperationId = "AddUserToProject")]
        public async Task<ActionResult<DetailedUser>> AddUserToProjectAsync(string username, Project project)
        {
            if (project == null)
            {
                return BadRequest();
            }

            var user = await _userSearchService.SearchAsync(username);

            if (user != null)
            {
                var success = await _userManagementService.AddUserToProjectAsync(user, project);

                var projects = await _userManagementService.GetProjectsForUserAsync(user);

                DetailedUser result = CreateDetailedUser(user, projects);

                return result;
            }

            return NotFound();
        }

        /// <summary>Removes the user from a project.</summary>
        /// <param name="username">The username.</param>
        /// <param name="project">The project.</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{username}")]
        [SwaggerOperation(OperationId = "RemoveUserFromProject")]
        public async Task<ActionResult<DetailedUser>> RemoveUserFromProjectAsync(string username, Project project)
        {
            if (project == null)
            {
                return BadRequest();
            }

            var user = await _userSearchService.SearchAsync(username);

            if (user != null)
            {
                var success = await _userManagementService.RemoveUserFromProjectAsync(user, project);

                var projects = await _userManagementService.GetProjectsForUserAsync(user);

                DetailedUser result = CreateDetailedUser(user, projects);

                return result;
            }

            return NotFound();
        }

        private DetailedUser CreateDetailedUser(User user, IEnumerable<Project> projects)
        {
            DetailedUser detailedUser = new DetailedUser
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName,
                Enabled = true,
                Projects = projects.ToArray()
            };

            return detailedUser;
        }
    }
}
