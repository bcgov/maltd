using BcGov.Jag.AccountManagement.Server.Infrastructure;
using BcGov.Jag.AccountManagement.Server.Models;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Services;
using FlatFiles;
using FlatFiles.TypeMapping;
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
        public Response(List<DynamicsUserAccessStatus> records)
        {
            var mapper = DynamicsUserAccessStatus.GetMapper();

            using var writer = new StringWriter();
            var options = new DelimitedOptions() { IsFirstRecordSchema = true };
            mapper.Write(writer, records, options);
            Report = Encoding.UTF8.GetBytes(writer.ToString());
        }

        public byte[] Report { get; set; }
    }

    public class Handler : IRequestHandler<Request, Response>
    {
        private readonly ProjectConfigurationCollection _projects;
        private readonly IUserManagementService _userManagementService;
        private readonly IUserSearchService _userSearchService;
        private readonly ILogger<Handler> _logger;

        public Handler(
            ProjectConfigurationCollection projects,
            IUserManagementService userManagementService,
            IUserSearchService userSearchService,
            ILogger<Handler> logger)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            using var activity = Diagnostics.Source.StartActivity("User Access Report");

            var records = await GetRecordsAsync(cancellationToken);
            return new Response(records);
        }

        private async Task<List<DynamicsUserAccessStatus>> GetRecordsAsync(CancellationToken cancellationToken)
        {
            Dictionary<string, ActiveDirectoryUserStatus> userStatus = new(StringComparer.OrdinalIgnoreCase);

            List<DynamicsUserAccessStatus> records = new();

            foreach (ProjectConfiguration project in _projects)
            {
                foreach (ProjectResource resource in project.Resources.Where(_ => _.Type == ProjectType.Dynamics))
                {
                    IList<UserStatus> users = await _userManagementService.GetUsersAsync(project, resource, cancellationToken);

                    foreach (var user in users.Where(_ => !string.IsNullOrEmpty(_.Username)))
                    {
                        if (!user.Username.StartsWith("IDIR\\", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation("{Username} does not appear to be an IDIR. We can only process IDIR accounts", user.Username);
                            continue;
                        }

                        DynamicsUserAccessStatus record = new()
                        {
                            ProjectName = project.Name,
                            ProjecAccountDisabled = user.IsDisabled,
                            ActiveDirectoryAccount = user.Username
                        };

                        records.Add(record);


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

                        if (status is not null)
                        {
                            record.ActiveDirectoryAccountExists = true;
                            record.ActiveDirectoryCompany = status.Company;
                            record.ActiveDirectoryDisplayName = status.DisplayName;
                            record.ActiveDirectoryEmail = status.Email;
                            record.ActiveDirectoryAccountDisabled = status.UserAccountControl.HasFlag(UserAccountControl.AccountDisabled);
                            record.ActiveDirectoryPasswordExpired = status.UserAccountControl.HasFlag(UserAccountControl.PasswordExpired);
                            record.ActiveDirectoryLockedout = status.UserAccountControl.HasFlag(UserAccountControl.Lockout);
                        }
                        else
                        {
                            record.ActiveDirectoryAccountExists = false;
                        }
                    }
                }
            }

            return records;
        }

    }
}

public class DynamicsUserAccessStatus
{
    public string ProjectName { get; set; }
    public bool ProjecAccountDisabled { get; set; }

    public bool ActiveDirectoryAccountExists { get; set; }

    public string? ActiveDirectoryAccount { get; set; }
    public string? ActiveDirectoryDisplayName { get; set; }
    public string? ActiveDirectoryEmail { get; set; }
    public string? ActiveDirectoryCompany { get; set; }

    public bool? ActiveDirectoryAccountDisabled { get; set; }
    public bool? ActiveDirectoryPasswordExpired { get; set; }
    public bool? ActiveDirectoryLockedout { get; set; }

    public static IDelimitedTypeMapper<DynamicsUserAccessStatus> GetMapper()
    {
        IDelimitedTypeMapper<DynamicsUserAccessStatus>? mapper = DelimitedTypeMapper.Define<DynamicsUserAccessStatus>();

        mapper.Property(c => c.ProjectName).ColumnName("Dynamics Project");
        mapper.Property(c => c.ActiveDirectoryAccount).ColumnName("AD Account");
        mapper.Property(c => c.ProjecAccountDisabled).ColumnName("Dynamics Account Disabled");

        mapper.Property(c => c.ActiveDirectoryDisplayName).ColumnName("AD Display Name");
        mapper.Property(c => c.ActiveDirectoryEmail).ColumnName("AD Email");
        mapper.Property(c => c.ActiveDirectoryCompany).ColumnName("AD Company");
        mapper.Property(c => c.ActiveDirectoryAccountDisabled).ColumnName("AD Account Disabled");
        mapper.Property(c => c.ActiveDirectoryPasswordExpired).ColumnName("AD Password Expired");
        mapper.Property(c => c.ActiveDirectoryLockedout).ColumnName("AD Account Locked Out");

        return mapper;
    }
}
