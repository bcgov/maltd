using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Models.Dynamics;
using Serilog;
using Simple.OData.Client;

namespace BcGov.Malt.Web.Services
{
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

            SystemUser entry = await GetSystemUserByLogon(client, logon, cancellationToken);

            if (entry == null)
            {
                Logger.Information("{Username} does not exist, creating a new record", username);

                User user = await _userSearchService.SearchAsync(username);
                BusinessUnit rootBusinessUnit = await GetRootBusinessUnit(client, cancellationToken);

                // populate the SystemUser with required attributes
                entry = new SystemUser
                {
                    Firstname = user.FirstName,
                    Lastname = user.LastName,
                    DomainName = IDIR.Logon(username),
                    InternalEMailAddress = user.Email,
                    BusinessUnit = rootBusinessUnit,
                    IsDisabled = false
                };

                await client
                    .For<SystemUser>()
                    .Set(entry)
                    .InsertEntryAsync(cancellationToken);
            }
            else if (entry.IsDisabled != null && entry.IsDisabled.Value)
            {
                Logger.Information("{@SystemUser} exists but is disabled, enabling user", entry);
                await UpdateSystemUserDisableFlag(client, entry.SystemUserId, false, cancellationToken);
            }
            else
            {
                Logger.Information("{@SystemUser} exists and is already enabled user", entry);
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
            SystemUser entry = await GetSystemUserByLogon(client, logon, cancellationToken);

            if (entry == null)
            {
                Logger.Information("User {Username} was not found in Dynamics, will not perform update", username);
                return string.Empty;
            }

            if (entry.IsDisabled.HasValue && entry.IsDisabled.Value)
            {
                Logger.Information("User {@SystemUser} is already disabled in Dynamics, will not perform update", new { entry.DomainName, entry.IsDisabled, entry.SystemUserId });
                return string.Empty; // user does not exist, or is already disabled
            }

            await UpdateSystemUserDisableFlag(client, entry.SystemUserId, true, cancellationToken);

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
            var entry = await GetSystemUserByLogon(client, logon, cancellationToken);

            return entry?.IsDisabled != null && !entry.IsDisabled.Value;
        }

        private Task<IDictionary<string, object>> UpdateSystemUserDisableFlag(IODataClient client, Guid systemUserId, bool isDisabled, CancellationToken cancellationToken)
        {
            Logger.Debug("Updating  SystemUser with {SystemUserId} with isDisabled flag set to {UsDisabled} in Dynamics", systemUserId, isDisabled);

            // using the untyped version because the typed version was giving an error:
            //
            // Microsoft.OData.ODataException: The property 'isdisabled' does not exist on type 'Microsoft.Dynamics.CRM.systemuser'.
            // Make sure to only use property names that are defined by the type or mark the type as open type.
            //
            return client
                .For("systemuser")
                .Key(systemUserId)
                .Set(new { isdisabled = isDisabled })
                .UpdateEntryAsync(cancellationToken);
        }

        private async Task<SystemUser> GetSystemUserByLogon(IODataClient client, string logon, CancellationToken cancellationToken)
        {
            Logger.Debug("Getting Dynamics SystemUser with {DomainName}", logon);

            try
            {
                SystemUser systemUser = await client
                    .For<SystemUser>()
                    .Filter(_ => _.DomainName == logon)
                    .FindEntryAsync(cancellationToken);

                if (systemUser != null)
                {
                    Logger.Debug("Found Dynamics SystemUser {@SystemUser}",
                        new {systemUser.DomainName, systemUser.IsDisabled, systemUser.SystemUserId});
                }
                else
                {
                    Logger.Debug("Dynamics SystemUser with {DomainName} not found", logon);
                }

                return systemUser;
            }
            catch (TaskCanceledException exception)
            {
                // timeout
                Logger.Warning(exception, "Task was cancelled looking up dynamics SystemUser with {DomainName}", logon);
                throw;
            }
            catch (Exception exception)
            {
                // we have seen TaskCanceledException being thrown in testing
                Logger.Warning(exception, "An error was thrown looking up dynamics SystemUser with {DomainName}", logon);
                throw; // return null?
            }
        }

        private async Task<BusinessUnit> GetRootBusinessUnit(IODataClient client, CancellationToken cancellationToken)
        {
            Logger.Debug("Getting root business unit from dynamics");

            // should be only one, however, get all just in case
            IEnumerable<BusinessUnit> entries = await client
                .For<BusinessUnit>()
                .Filter(_ => _.ParentBusinessUnit == null)
                .FindEntriesAsync(cancellationToken);

            BusinessUnit businessUnit = null;

            foreach (var entry in entries)
            {
                if (businessUnit == null)
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

        private IODataClient GetODataClient() => _factory.Create(Project.Id + "-dynamics");
    }
}
