using BcGov.Jag.AccountManagement.Server.Models;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Models.Dynamics;
using ILogger = Serilog.ILogger;
using Simple.OData.Client;
using BcGov.Jag.AccountManagement.Shared;
using BcGov.Jag.AccountManagement.Server.Infrastructure;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class DynamicsResourceUserManagementService : ResourceUserManagementService
{
    private readonly IODataClientFactory _factory;

    public DynamicsResourceUserManagementService(ProjectConfiguration project,
        ProjectResource projectResource,
        IODataClientFactory factory,
        ILogger logger)
        : base(project, projectResource, logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public override async Task<string> AddUserAsync(User user, CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Add User To Project");
        activity?.AddTag("project.name", Project.Name);
        activity?.AddTag("project.type", "Dynamics");

        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(user));
        }

        IODataClient client = GetODataClient();
        string logon = IDIR.Logon(user.UserName);

        Logger.Debug("Adding {Username} to project", user.UserName);

        SystemUser entry = await client
            .For<SystemUser>()
            .Filter(_ => _.DomainName == logon)
            .Select(_ => _.SystemUserId)
            .FindEntryAsync(cancellationToken);

        if (entry == null)
        {
            Logger.Information("{Username} does not exist, creating a new record", user.UserName);

            BusinessUnit rootBusinessUnit = await GetRootBusinessUnit(client, cancellationToken);

            // populate the SystemUser with required attributes
            entry = new SystemUser
            {
                Firstname = user.FirstName,
                Lastname = user.LastName,
                DomainName = logon,
                InternalEMailAddress = user.Email,
                BusinessUnit = rootBusinessUnit,
                IsDisabled = false,
                SharePointEmailAddress = user.UserPrincipalName
            };

            await client
                .For<SystemUser>()
                .Set(entry)
                .InsertEntryAsync(cancellationToken);
        }
        else
        {
            Logger.Information("{@SystemUser} exists ensuring the user is enabled", entry);
            await UpdateSystemUserDisableFlag(client, entry.SystemUserId, user, false, cancellationToken);
        }

        return string.Empty;
    }

    public override async Task<string> RemoveUserAsync(User user, CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Remove User From Project");
        activity?.AddTag("project.name", Project.Name);
        activity?.AddTag("project.type", "Dynamics");

        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(user));
        }

        IODataClient client = GetODataClient();
        string logon = IDIR.Logon(user.UserName);

        Logger.Debug("Removing {Username} from project", user.UserName);

        SystemUser entry = await client
            .For<SystemUser>()
            .Filter(_ => _.DomainName == logon)
            .Select(_ => _.SystemUserId)
            .FindEntryAsync(cancellationToken);

        if (entry == null)
        {
            Logger.Information("User {Username} was not found in Dynamics, will not perform update", user.UserName);
            return string.Empty;
        }

        Logger.Information("{@SystemUser} exists ensuring the user is disabled", entry);
        await UpdateSystemUserDisableFlag(client, entry.SystemUserId, user: null, true, cancellationToken);

        return string.Empty;
    }

    public override async Task<bool> UserHasAccessAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(user));
        }

        using var userHasAccessActivity = Diagnostics.Source.StartActivity("Check User Has Access");
        userHasAccessActivity?.AddTag("project.name", this.Project.Name);
        userHasAccessActivity?.AddTag("project.type", "Dynamics");

        IODataClient client = GetODataClient();

        string logon = IDIR.Logon(user.UserName);

        Logger.Debug("Checking if user has access to project");

        SystemUser entry = await client
            .For<SystemUser>()
            .Filter(_ => _.DomainName == logon && _.IsDisabled == false)
            .Select(_ => _.SystemUserId)
            .FindEntryAsync(cancellationToken);

        return entry != null;
    }

    private Task<IDictionary<string, object>> UpdateSystemUserDisableFlag(IODataClient client, Guid systemUserId, User user, bool isDisabled, CancellationToken cancellationToken)
    {
        Logger.Debug("Updating  SystemUser with {SystemUserId} with isDisabled flag set to {UsDisabled} in Dynamics", systemUserId, isDisabled);

        object value;

        if (isDisabled || user == null)
        {
            value = new {isdisabled = isDisabled};
        }
        else
        {
            // on add, we want to update the attributes of the user (note the sharepointemailaddress may not have previously set)
            value = new
            {
                firstname = user.FirstName,
                lastname = user.LastName,
                isdisabled = isDisabled,
                sharepointemailaddress = user.UserPrincipalName
            };
        }

        // using the untyped version because the typed version was giving an error:
        //
        // Microsoft.OData.ODataException: The property 'isdisabled' does not exist on type 'Microsoft.Dynamics.CRM.systemuser'.
        // Make sure to only use property names that are defined by the type or mark the type as open type.

        return client
            .For("systemuser")
            .Key(systemUserId)
            .Set(value)
            .UpdateEntryAsync(cancellationToken);
    }

    private async Task<BusinessUnit> GetRootBusinessUnit(IODataClient client, CancellationToken cancellationToken)
    {
        Logger.Debug("Getting root business unit from dynamics");

        // should be only one, however, get all just in case
        IEnumerable<BusinessUnit> entries = await client
            .For<BusinessUnit>()
            .Filter(_ => _.ParentBusinessUnit == null)
            .Select(_ => _.BusinessUnitId)
            .FindEntriesAsync(cancellationToken);

        BusinessUnit? businessUnit = null;

        foreach (var entry in entries)
        {
            if (businessUnit is null)
            {
                businessUnit = entry;
            }
            else
            {
                // log found extra root business unit
                Logger.Information("Found duplicate root {@DuplicateBusinessUnit}, the first root {@BusinessUnit} will be used",
                    entry,
                    businessUnit);
            }
        }

        if (businessUnit == null)
        {
            Logger.Warning("No root business unit found where ParentBusinessUnit is null");
        }

        return businessUnit;
    }

    private IODataClient GetODataClient() => _factory.Create(Project.Name + "-dynamics");

    public override async Task<IList<UserStatus>> GetUsersAsync(CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Get Dynamics Users");

        IODataClient client = GetODataClient();

        var users = await client
            .For<SystemUser>()
            .Select(_ => new { _.DomainName, _.IsDisabled })
            .FindEntriesAsync(cancellationToken);

        return users
            .Select(_ => new UserStatus { Username = _.DomainName, IsDisabled = _.IsDisabled.Value })
            .ToList();

    }
}

public class UserStatus
{
    public string Username { get; set; }
    public bool IsDisabled { get; set; }
}