using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Services;
using BcGov.Jag.AccountManagement.Shared;
using MediatR;

namespace BcGov.Jag.AccountManagement.Server.Features.Users.Lookup;

public class Handler : IRequestHandler<Request, DetailedUser?>
{
    private readonly IUserSearchService _userSearchService;
    private readonly IUserManagementService _userManagementService;
    private readonly ILogger<Handler> _logger;

    public Handler(IUserSearchService userSearchService, IUserManagementService userManagementService, ILogger<Handler> logger)
    {
        _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
        _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<DetailedUser?> Handle(Request request, CancellationToken cancellationToken)
    {
        User? user = null;

        try
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            _logger.LogDebug("Searching for user {Username}", request.Username);

            user = await _userSearchService.SearchAsync(request.Username, cancellationToken);

            if (user is null)
            {
                _logger.LogDebug("Lookup for {Username} returned null, returning null", request.Username);
                return null;
            }

            _logger.LogDebug("Looking for project membership for user {Username}", user.UserName);
            var getProjectsResult = await _userManagementService.GetProjectsForUserAsync(user, cts.Token);

            if (getProjectsResult.IsSuccess)
            {
                var projects = getProjectsResult.Value;
                DetailedUser result = new DetailedUser(user, projects);

                return result;
            }

            // TODO handle the failures
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error lookup user");
            throw;
        }
    }
}