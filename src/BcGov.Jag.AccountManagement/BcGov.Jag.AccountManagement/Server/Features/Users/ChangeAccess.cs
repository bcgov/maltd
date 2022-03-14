using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Services;
using BcGov.Jag.AccountManagement.Shared;
using MediatR;

namespace BcGov.Jag.AccountManagement.Server.Features.Users;

public static class ChangeAccess
{
    public class Request : IRequest<Unit>
    {
        public Request(string username, IList<ProjectMembershipModel> projectMemberships)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            ProjectMemberships = projectMemberships ?? throw new ArgumentNullException(nameof(projectMemberships));
        }

        public string Username { get; init; }
        public IList<ProjectMembershipModel> ProjectMemberships { get; init; }
    }

    public class Handler : IRequestHandler<Request, Unit>
    {
        private readonly ProjectConfigurationCollection _projects;
        private readonly IUserManagementService _userManagementService;

        public Handler(ProjectConfigurationCollection projects, IUserManagementService userManagementService)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
        }

        public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
        {
            // TODO: change this to be parallel
            foreach (var projectMembership in request.ProjectMemberships)
            {
                var project = _projects.Single(_ => _.Name == projectMembership.ProjectName);
                await _userManagementService.ChangeUserProjectAccessAsync(request.Username, project, projectMembership, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
