using System.Net.Http;
using System;
using BcGov.Jag.AccountManagement.Server.Features.Reports;
using BcGov.Jag.AccountManagement.Server.Features.Users;
using BcGov.Jag.AccountManagement.Server.Services;
using BcGov.Jag.AccountManagement.Shared;
using BcGov.Jag.AccountManagement.Shared.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Simple.OData.Client;

namespace BcGov.Jag.AccountManagement.Server.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserSearchService _service;
    private readonly IMediator _mediator;
    private readonly ILogger<UserController> _logger;
    private readonly IServiceProvider _serviceProvider;

    public UserController(IUserSearchService service, IMediator mediator, ILogger<UserController> logger, IServiceProvider serviceProvider)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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

    [HttpPost]
    [Route("UpdateUserProjects/validate")]
    public async Task<IActionResult> validateProjectConfigAsync(ProjectResource projectResource, CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient();
        var logger = _serviceProvider.GetRequiredService<ILogger<OAuthClient>>();
        OAuthClient oAuthClient = new OAuthClient(httpClient, logger);

        var options = new OAuthOptions
        {
            AuthorizationUri = projectResource.AuthorizationUri,
            ClientId = projectResource.ClientId,
            ClientSecret = projectResource.ClientSecret,
            Username = projectResource.Username,
            Password = projectResource.Password,
            Resource = projectResource.Resource
        };
        Token token = await oAuthClient.GetTokenAsync(options, cancellationToken);
        logger.LogInformation("token generated");
        // ChangeAccess.Request request = new ChangeAccess.Request(username, projectMembership);
        // await _mediator.Send(request, cancellationToken);
        if (token != null)
        {

            // DynamicsODataClientFactory factory = new DynamicsODataClientFactory();

            // IODataClient oDataClient = factory.Create("");
            // if (oDataClient != null)
            // {
            //     return Ok();
            // }
            return Ok();
        }
        return BadRequest();
    }
    
    [AllowAnonymous]
    [HttpGet]
    [Route("Report")]
    public async Task<IActionResult> GetUserReportAsync(CancellationToken cancellationToken)
    {
        UserMembershipReport.Request request = new UserMembershipReport.Request();
        UserMembershipReport.Response response = await _mediator.Send(request, cancellationToken);
        
        return File(response.Report, "text/csv", "dynamics-user-report.csv");
    }
}
