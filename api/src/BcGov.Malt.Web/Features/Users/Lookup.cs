﻿using System;
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
#pragma warning disable CA1034 // do not next type
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

        public class Handler : RequestHandlerBase<Request, DetailedUser>
        {
            private readonly IUserSearchService _userSearchService;
            private readonly IUserManagementService _userManagementService;

            public Handler(IUserSearchService userSearchService, IUserManagementService userManagementService, ILogger<Handler> logger)
                : base(logger)
            {
                _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
                _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
            }

            public override async Task<DetailedUser> Handle(Request request, CancellationToken cancellationToken)
            {
                try
                {
                    using CancellationTokenSource cts = CreateCancellationTokenSource(cancellationToken);

                    if (request is null)
                    {
                        throw new ArgumentNullException(nameof(request));
                    }

                    Logger.LogDebug("Searching for user {Username}", request.Username);

                    var user = await _userSearchService.SearchAsync(request.Username);

                    if (user == null)
                    {
                        Logger.LogDebug("Lookup for {Username} returned null, returning null", request.Username);
                        return null;
                    }

                    Logger.LogDebug("Looking for project membership for user {Username}", user.UserName);
                    var projects = await _userManagementService.GetProjectsForUserAsync(user, cts.Token);

                    DetailedUser result = new DetailedUser(user, projects);

                    return result;
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Error lookup user");
                    throw;
                }
            }
        }
    }
}
