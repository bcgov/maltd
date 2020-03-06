using System;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Features.Users
{
    public class Lookup
    {
        public class Request : IRequest<DetailedUser>
        {
            public Request(string username)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Argument cannot be null or empty", nameof(username));
                }

                Username = username.Trim();
            }

            public string Username { get; }
        }

        public class Handler : IRequestHandler<Request, DetailedUser>
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

            public async Task<DetailedUser> Handle(Request request, CancellationToken cancellationToken)
            {
                if (request is null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                _logger.LogDebug("Searching for user {Username}", request.Username);

                var user = await _userSearchService.SearchAsync(request.Username);

                if (user == null)
                {
                    _logger.LogDebug("Lookup for {username} returned null, returning null", request.Username);
                    return null;
                }

                _logger.LogDebug("Looking for project membership for user {Username}", user.UserName);
                var projects = await _userManagementService.GetProjectsForUserAsync(user);

                DetailedUser result = new DetailedUser(user, projects);

                return result;
            }
        }
    }
}
