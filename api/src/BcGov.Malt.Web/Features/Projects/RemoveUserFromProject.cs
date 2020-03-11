using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Services;
using MediatR;
using Microsoft.Extensions.Logging;

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
            private readonly IProjectService _projectService;
            private readonly ILogger<Handler> _logger;

            public Handler(IUserSearchService userSearchService, IUserManagementService userManagementService, IProjectService projectService, ILogger<Handler> logger)
            {
                _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
                _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
                _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<bool> Handle(Request request, CancellationToken cancellationToken)
            {
                var projects = await _projectService.GetProjectsAsync();

                var project = projects.SingleOrDefault(_ => _.Id == request.ProjectId);

                if (project == null)
                {
                    _logger.LogInformation("Project {ProjectId} not found", request.ProjectId);
                    return false;
                }

                var user = await _userSearchService.SearchAsync(request.Username);
                if (user == null)
                {
                    _logger.LogInformation("User {Username} not found", request.Username);
                    return false;
                }

                return await _userManagementService.RemoveUserFromProjectAsync(user, project);
            }
        }
    }
}
