using System;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Services;
using MediatR;

namespace BcGov.Malt.Web.Features.Projects
{
    public static class RemoveUserFromProject
    {
        public class Request : IRequest<bool>
        {
            public Request(string projectId, string username)
            {
                ProjectId = projectId ?? throw new ArgumentNullException(nameof(projectId));
                Username = username ?? throw new ArgumentNullException(nameof(username));
            }

            public string ProjectId { get; }
            public string Username { get; }
        }

        public class Handler : IRequestHandler<Request, bool>
        {
            private readonly IUserSearchService _userSearchService;
            private readonly IUserManagementService _userManagementService;

            public Handler(IUserSearchService userSearchService, IUserManagementService userManagementService)
            {
                _userSearchService = userSearchService;
                _userManagementService = userManagementService;
            }

            public Task<bool> Handle(Request request, CancellationToken cancellationToken)
            {
                return Task.FromResult(false);
            }
        }
    }
}
