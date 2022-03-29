using BcGov.Jag.AccountManagement.Server.Models;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Services;
using MediatR;
using System.Text;

namespace BcGov.Jag.AccountManagement.Server.Features.Reports;

public class UserMembershipReport
{
    public class Request : IRequest<Response>
    {
    }

    public class Response
    {
        public Response(List<List<string>> rows)
        {
            StringBuilder buffer = new StringBuilder();
            foreach (var row in rows)
            {
                buffer.Append(string.Join(",", row));
                buffer.AppendLine();
            }

            Report = buffer.ToString();
        }

        public string Report { get; set; }
    }

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ProjectConfigurationCollection _projects;
        private readonly IUserManagementService _userManagementService;
        private readonly IUserSearchService _userSearchService;

        public Handler(
            ProjectConfigurationCollection projects, 
            IUserManagementService userManagementService,
            IUserSearchService userSearchService)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            Dictionary<string, ActiveDirectoryUserStatus> userStatus = new Dictionary<string, ActiveDirectoryUserStatus>(StringComparer.OrdinalIgnoreCase);

            List<Tuple<ProjectConfiguration, ProjectResource, UserStatus>> results = new List<Tuple<ProjectConfiguration, ProjectResource, UserStatus>>();

            foreach (ProjectConfiguration project in _projects)
            {
                foreach (ProjectResource resource in project.Resources.Where(_ => _.Type == ProjectType.Dynamics))
                {
                    IList<UserStatus> users = await _userManagementService.GetUsersAsync(project, resource, cancellationToken);

                    foreach (var user in users.Where(_ => !string.IsNullOrEmpty(_.Username)))
                    {
                        results.Add(Tuple.Create(project, resource, user));

                        // avoid looking up active directory user multiple times if they exist in multiple projects
                        if (!userStatus.TryGetValue(user.Username, out var status))
                        {
                            var username = IDIR.Username(user.Username);
                            status = await _userSearchService.GetAccountStatusAsync(username, cancellationToken);
                            if (status is not null)
                            {
                                userStatus.Add(user.Username, status);
                            }
                        }
                    }
                }
            }

            List<List<string>> rows = new List<List<string>>();

            List<string> header = new List<string>();
            header.Add("IDIR Username");
            header.Add("Project Name");
            header.Add("Resource Type");
            header.Add("Resource Account Disabled");

            header.Add("IDIR Account");
            header.Add("Is Account Disabled");
            header.Add("Is Password Expired");
            header.Add("Is Account Locked Out");

            rows.Add(header);

            foreach (var result in results)
            {
                List<string> row = new List<string>();
                row.Add(result.Item1.Name); // Project Name
                row.Add(result.Item2.Type.ToString()); // Resource Type
                row.Add(result.Item3.Username);
                row.Add(result.Item3.IsDisabled.ToString());



                if (userStatus.TryGetValue(result.Item3.Username, out var status))
                {
                    row.Add("Exists");
                    row.Add(status.UserAccountControl.HasFlag(UserAccountControl.AccountDisabled).ToString());
                    row.Add(status.UserAccountControl.HasFlag(UserAccountControl.PasswordExpired).ToString());
                    row.Add(status.UserAccountControl.HasFlag(UserAccountControl.Lockout).ToString());
                }
                else
                {
                    row.Add("Not Found");
                    row.Add(String.Empty);
                    row.Add(String.Empty);
                    row.Add(String.Empty);
                }

                rows.Add(row);
            }

            return new Response(rows);
        }
    }
}
