using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Features.Users
{
    public static class Search
    {
        public class Request : IRequest<User>
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


        public class Handler : IRequestHandler<Request, User>
        {
            private readonly IUserSearchService _userSearchService;
            private readonly ILogger<Handler> _logger;

            public Handler(IUserSearchService userSearchService, ILogger<Handler> logger)
            {
                _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<User> Handle(Request request, CancellationToken cancellationToken)
            {
                if (request is null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                _logger.LogDebug("Searching for user {Username}", request.Username);

                var user = await _userSearchService.SearchAsync(request.Username);

                return user;
            }
        }
    }
}
