using BcGov.Jag.AccountManagement.Server.Models;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Models.Dynamics;
using ILogger = Serilog.ILogger;
using Simple.OData.Client;
using BcGov.Jag.AccountManagement.Shared;

namespace BcGov.Jag.AccountManagement.Server.Services;

public class DynamicsResourceUserManagementService : ResourceUserManagementService
{
    private readonly IODataClientFactory _factory;
    private readonly IUserSearchService _userSearchService;

    public DynamicsResourceUserManagementService(ProjectConfiguration project,
        ProjectResource projectResource,
        IODataClientFactory factory,
        IUserSearchService userSearchService,
        ILogger logger)
        : base(project, projectResource, logger)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
    }

    public override async Task<string> AddUserAsync(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        }

        IODataClient client = GetODataClient();
        string logon = IDIR.Logon(username);

        Logger.Debug("Adding {Username} to project", username);

        User user = await _userSearchService.SearchAsync(username, cancellationToken);

        SystemUser entry = await client
            .For<SystemUser>()
            .Filter(_ => _.DomainName == logon)
            .Select(_ => _.SystemUserId)
            .FindEntryAsync(cancellationToken);

        if (entry == null)
        {
            Logger.Information("{Username} does not exist, creating a new record", username);

            BusinessUnit rootBusinessUnit = await GetRootBusinessUnit(client, cancellationToken);

            // populate the SystemUser with required attributes
            entry = new SystemUser
            {
                Firstname = user.FirstName,
                Lastname = user.LastName,
                DomainName = IDIR.Logon(username),
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

    public override async Task<string> RemoveUserAsync(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        }

        IODataClient client = GetODataClient();
        string logon = IDIR.Logon(username);

        Logger.Debug("Removing {Username} from project", username);

        SystemUser entry = await client
            .For<SystemUser>()
            .Filter(_ => _.DomainName == logon)
            .Select(_ => _.SystemUserId)
            .FindEntryAsync(cancellationToken);

        if (entry == null)
        {
            Logger.Information("User {Username} was not found in Dynamics, will not perform update", username);
            return string.Empty;
        }

        Logger.Information("{@SystemUser} exists ensuring the user is disabled", entry);
        await UpdateSystemUserDisableFlag(client, entry.SystemUserId, user: null, true, cancellationToken);

        return string.Empty;
    }

    public override async Task<bool> UserHasAccessAsync(string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        }

        IODataClient client = GetODataClient();

        string logon = IDIR.Logon(username);

        Logger.Debug("Checking {Username} has access to project", username);

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
}
