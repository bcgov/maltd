using BcGov.Jag.AccountManagement.Server.Services;
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
    private readonly ILogger<UserController> _logger;

    public UserController(IUserSearchService service, ILogger<UserController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IActionResult> SearchAsync([FromQuery(Name = "q")][Required] string query, CancellationToken cancellationToken)
    {
        var user = await _service.SearchAsync(query);

        if (user is not null)
        {
            return Ok(user);
        }

        return NotFound();
    }
}
