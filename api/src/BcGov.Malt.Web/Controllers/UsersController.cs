using System;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Features.Users;
using MediatR;
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
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;

        /// <summary>Initializes a new instance of the <see cref="UsersController" /> class.</summary>
        /// <param name="mediator"></param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="mediator" /> or <paramref name="logger" /> is null.
        /// </exception>
        public UsersController(IMediator mediator, ILogger<UsersController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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

            if (string.IsNullOrEmpty(query))
            {
                _logger.LogInformation("Required parameter {Parameter} was not specified, returning 400 Bad Request", nameof(query));
                return BadRequest();
            }

            _logger.LogDebug("Searching for user using {Query}", query);

            var user = await _mediator.Send(new Search.Request(query));

            if (user == null)
            {
                _logger.LogDebug("Search for {Username} returned null, returning 404 Not Found", query);
                return NotFound();
            }

            return user;
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

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogInformation("Required parameter {parameter} was not specified, returning 400 Bad Request", nameof(username));
                return BadRequest();
            }

            _logger.LogDebug("Looking up user {Username}", username);

            DetailedUser result = await _mediator.Send(new Lookup.Request(username));

            return result;
        }
    }
}
