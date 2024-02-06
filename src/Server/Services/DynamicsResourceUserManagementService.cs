using BcGov.Jag.AccountManagement.Server.Models;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Models.Dynamics;
using ILogger = Serilog.ILogger;
using Simple.OData.Client;
using BcGov.Jag.AccountManagement.Shared;
using BcGov.Jag.AccountManagement.Server.Infrastructure;
using System.Text.Json.Serialization;
using FluentResults;
using System.Collections.Generic;

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

    public override async Task<Result<string>> AddUserAsync(User user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

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

        Result<BusinessUnit> rootBusinessUnitResult = await GetRootBusinessUnit(client, cancellationToken);
        if (rootBusinessUnitResult.IsFailed)
        {
            // return
        }

        BusinessUnit rootBusinessUnit = rootBusinessUnitResult.Value;

        var systemAdministratorRoleResult = await GetSystemAdministratorRole(client, rootBusinessUnit, cancellationToken);
        if (systemAdministratorRoleResult.IsFailed)
        {
            // return
        }

        var systemAdministratorRole = systemAdministratorRoleResult.Value;

        SystemUser? entry = null;

        try
        {
            entry = await client
                .For<SystemUser>()
                .Filter(_ => _.DomainName == logon)
                .Select(_ => new { _.SystemUserId })
                .FindEntryAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OAuthClientException exception)
        {
            return OnOAuthClientException<string>(exception);
        }
        catch (WebRequestException exception)
        {
            return OnWebRequestException<string>(exception);
        }
        catch (OAuthException exception)
        {
            return Result.Fail(exception.Message);
        }

        if (entry is null)
        {
            Logger.Information("{Username} does not exist, creating a new record", user.UserName);

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

            try
            {
                entry = await client
                    .For<SystemUser>()
                    .Set(entry)
                    .InsertEntryAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (OAuthClientException exception)
            {
                return OnOAuthClientException<string>(exception);
            }
            catch (WebRequestException exception)
            {
                return OnWebRequestException<string>(exception);
            }
            catch (Exception exception)
            {
                return Result.Fail(exception.Message);
            }
        }
        else
        {
            Logger.Information("{@SystemUser} exists ensuring the user is enabled", entry);
            await UpdateSystemUserDisableFlag(client, entry.SystemUserId, user, false, cancellationToken);
        }

        if (systemAdministratorRole is not null)
        {
            Logger.Information("Adding user to system administrator role");
            await AddRoleToUserAsync(systemAdministratorRole.RoleId, entry.SystemUserId);
        }

        return Result.Ok(string.Empty);
    }

    class AssignRoleRequest
    {
        [JsonPropertyName("@odata.id")]
        public Uri Request { get; set; }
    }

    private async Task AddRoleToUserAsync(Guid roleId, Guid userId)
    {
        AssignRoleRequest assignRoleRequest = new AssignRoleRequest 
        { 
            Request = new Uri(ProjectResource.Resource + $"roles({roleId})") 
        };

        JsonContent requestContent = JsonContent.Create(assignRoleRequest);
        string requestUri = $"{ProjectResource.Resource.PathAndQuery}systemusers({userId})/systemuserroles_association/$ref";

        HttpClient client = _factory.CreateHttpClient(Project.Name + "-dynamics");
        var response = await client.PostAsync(requestUri, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            Logger
                .ForContext("ResponseContent", responseContent)
                .ForContext("StatusCode", response.StatusCode)
                .ForContext("UserId", userId)
                .ForContext("RoleId", roleId)
                .ForContext("Project", Project.Name)
                .ForContext("Resource", ProjectResource.Resource)
                .Information("Could not add role to user");            

            // TODO: this should return an error
        }
    }


    public override async Task<Result<string>> RemoveUserAsync(User user, CancellationToken cancellationToken)
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

        SystemUser? entry = null;

        try
        {
            entry = await client
                .For<SystemUser>()
                .Filter(_ => _.DomainName == logon)
                .Select(_ => _.SystemUserId)
                .FindEntryAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OAuthClientException exception)
        {
            return OnOAuthClientException<string>(exception);
        }
        catch (WebRequestException exception)
        {
            return OnWebRequestException<string>(exception);
        }
        catch (Exception exception)
        {
            return Result.Fail(exception.Message);
        }

        if (entry is null)
        {
            Logger.Information("User {Username} was not found in Dynamics, will not perform update", user.UserName);
            return string.Empty;
        }

        Logger.Information("{@SystemUser} exists ensuring the user is disabled", entry);
        await UpdateSystemUserDisableFlag(client, entry.SystemUserId, user: null, true, cancellationToken);

        return string.Empty;
    }

    public override async Task<Result<bool>> UserHasAccessAsync(User user, CancellationToken cancellationToken)
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

        SystemUser? entry = null;

        try
        {
            entry = await client
                .For<SystemUser>()
                .Filter(_ => _.DomainName == logon && _.IsDisabled == false)
                .Select(_ => new { _.SystemUserId })
                .FindEntryAsync(cancellationToken)
                .ConfigureAwait(false);

            return entry != null;
        }
        catch (OAuthClientException exception)
        {
            return OnOAuthClientException<bool>(exception);
        }
        catch (WebRequestException exception)
        {
            return OnWebRequestException<bool>(exception);
        }
        catch (Exception exception)
        {
            return Result.Fail(exception.Message);
        }
    }

    private Result<T> OnOAuthClientException<T>(OAuthClientException exception)
    {
        Guid errorId = Guid.NewGuid();
        Logger
            .ForContext("ClientId", exception.ClientId)
            .ForContext("Resource", exception.Resource)
            .ForContext("ErrorId", errorId)
            .Error(exception, "Authentication error");

        return Result.Fail(new InvalidClientOAuthError(exception, errorId));
    }

    private Result<T> OnWebRequestException<T>(WebRequestException exception)
    {
        Guid errorId = Guid.NewGuid();
        Logger
            .ForContext("Response", exception.Response)
            .ForContext("ErrorId", errorId)
            .Error(exception, "Web request exception, could indicate the api gateway configuration is invalid");

        return Result.Fail($"Request to dynamics failed. Check logs for ErrorId={errorId}");
    }

    private Task<IDictionary<string, object>> UpdateSystemUserDisableFlag(IODataClient client, Guid systemUserId, User user, bool isDisabled, CancellationToken cancellationToken)
    {
        Logger.Debug("Updating SystemUser with {SystemUserId} with isDisabled flag set to {UsDisabled} in Dynamics", systemUserId, isDisabled);

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

    private async Task<Result<BusinessUnit>> GetRootBusinessUnit(IODataClient client, CancellationToken cancellationToken)
    {
        Logger.Debug("Getting root business unit from dynamics");

        IEnumerable<BusinessUnit> entries = Enumerable.Empty<BusinessUnit>();

        try
        {
            // should be only one, however, get all just in case
            entries = await client
                .For<BusinessUnit>()
                .Filter(_ => _.ParentBusinessUnit == null)
                .Select(_ => _.BusinessUnitId)
                .FindEntriesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OAuthClientException exception)
        {
            return OnOAuthClientException<BusinessUnit>(exception);
        }
        catch (WebRequestException exception)
        {
            return OnWebRequestException<BusinessUnit>(exception);
        }
        catch (Exception exception)
        {
            return Result.Fail(exception.Message);
        }

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

        if (businessUnit is null)
        {
            Logger.Warning("No root business unit found where ParentBusinessUnit is null");
            return Result.Fail("Root business unit not found");
        }

        return businessUnit;
    }

    private async Task<Result<Role?>> GetSystemAdministratorRole(IODataClient client, BusinessUnit businessUnit, CancellationToken cancellationToken)
    {
        IEnumerable<Role> entries = Enumerable.Empty<Role>();

        try
        {
            // may be better just to filter by business unit, but this works
            entries = await client
                .For<Role>()
                .Expand(_ => _.BusinessUnit)
                .Filter(_ => _.Name == "System Administrator")
                .FindEntriesAsync()
                .ConfigureAwait(false);
        }
        catch (OAuthClientException exception)
        {
            return OnOAuthClientException<Role?>(exception);
        }
        catch (WebRequestException exception)
        {
            return OnWebRequestException<Role?>(exception);
        }
        catch (Exception exception)
        {
            return Result.Fail(exception.Message);
        }

        var roles = entries.Where(_ => _.BusinessUnit != null && _.BusinessUnit.BusinessUnitId == businessUnit.BusinessUnitId).ToList();
        if (roles.Count == 0)
        {
            Logger.Warning("Could not find {Role} in {BusinessUnit}", "System Administrator", businessUnit);
            return Result.Fail($"Could not find role 'System Administrator' in {businessUnit}");
        }
        if (roles.Count > 1)
        {
            Logger.Warning("Found {Count} {Role} roles, the first one will used", roles.Count, "System Administrator");
        }

        var role = roles.FirstOrDefault();
        return role;
    }

    private IODataClient GetODataClient() => _factory.Create(Project.Name + "-dynamics");

    public override async Task<Result<IList<UserStatus>>> GetUsersAsync(CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Get Dynamics Users");

        IODataClient client = GetODataClient();

        try
        {
            IEnumerable<SystemUser> users = await client
                .For<SystemUser>()
                .Select(_ => new { _.DomainName, _.IsDisabled })
                .FindEntriesAsync(cancellationToken)
                .ConfigureAwait(false);

            return users
                .Select(_ => new UserStatus { Username = _.DomainName, IsDisabled = _.IsDisabled.Value })
                .ToList();
        }
        catch (OAuthClientException exception)
        {
            return OnOAuthClientException<IList<UserStatus>>(exception);
        }
        catch (WebRequestException exception)
        {
            return OnWebRequestException<IList<UserStatus>>(exception);
        }
        catch (Exception exception)
        {
            Logger.Error(exception, "Failed to get users from dynamics");
            return Result.Fail(exception.Message);
        }

    }
}

public class UserStatus
{
    public string Username { get; set; }
    public bool IsDisabled { get; set; }
}