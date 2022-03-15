using BcGov.Jag.AccountManagement.Server.Features.Users;
using BcGov.Jag.AccountManagement.Server.Services;
using BcGov.Jag.AccountManagement.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BcGov.Jag.AccountManagement.Server.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserSearchService _service;
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserSearchService service, IMediator mediator, ILogger<UserController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> SearchAsync([FromQuery(Name = "q")][Required] string query, CancellationToken cancellationToken)
    {
        var user = await _service.SearchAsync(query, cancellationToken);

        if (user is not null)
        {
            return Ok(user);
        }

        return NotFound();
    }

    [HttpGet]
    [Route("{username}")]
    public async Task<IActionResult> LookupAsync(string username, CancellationToken cancellationToken)
    {
        // TODO: user model validation

        if (string.IsNullOrEmpty(username))
        {
            _logger.LogInformation("Required parameter {parameter} was not specified, returning 400 Bad Request", nameof(username));
            return BadRequest();
        }

        _logger.LogDebug("Looking up user {Username}", username);

        DetailedUser result = await _mediator.Send(new Features.Users.Lookup.Request(username), cancellationToken);

        if (result is not null)
        {
            return Ok(result);
        }

        return NotFound();
    }


    [HttpPost]
    [Route("UpdateUserProjects/{username}")]
    public async Task<IActionResult> UpdateUserProjectsAsync(string username, IList<ProjectMembershipModel> projectMembership, CancellationToken cancellationToken)
    {
        ChangeAccess.Request request = new ChangeAccess.Request(username, projectMembership);
        await _mediator.Send(request, cancellationToken);
        return Ok();
    }
}
