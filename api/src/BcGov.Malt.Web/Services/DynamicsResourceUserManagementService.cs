using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Models.Dynamics;
using Microsoft.Extensions.Logging;
using Simple.OData.Client;

namespace BcGov.Malt.Web.Services
{
    public class DynamicsResourceUserManagementService : ResourceUserManagementService
    {
        private readonly IODataClientFactory _factory;
        private readonly IUserSearchService _userSearchService;
        private readonly ILogger<DynamicsResourceUserManagementService> _logger;

        public DynamicsResourceUserManagementService(ProjectConfiguration project,
            ProjectResource projectResource,
            IODataClientFactory factory,
            IUserSearchService userSearchService,
            ILogger<DynamicsResourceUserManagementService> logger)
            : base(project, projectResource)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<string> AddUserAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            IODataClient client = GetODataClient();
            string logon = IDIR.Logon(username);

            SystemUser entry = await GetSystemUserByLogon(client, logon);

            if (entry == null)
            {
                _logger.LogInformation("{Username} does not exist, creating a new record", username);

                User user = await _userSearchService.SearchAsync(username);
                BusinessUnit rootBusinessUnit = await GetRootBusinessUnit(client);

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

                entry = await client
                    .For<SystemUser>()
                    .Set(entry)
                    .InsertEntryAsync();
            }
            else if (entry.IsDisabled != null && entry.IsDisabled.Value)
            {
                _logger.LogInformation("{@SystemUser} exists but is disabled, enabling user", entry);
                await UpdateSystemUserDisableFlag(client, entry.SystemUserId, false);
            }
            else
            {
                _logger.LogInformation("{@SystemUser} exists and is already enabled user", entry);
            }

            return string.Empty;
        }

        public override async Task<string> RemoveUserAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            IODataClient client = GetODataClient();
            string logon = IDIR.Logon(username);

            SystemUser entry = await GetSystemUserByLogon(client, logon);

            if (entry == null)
            {
                _logger.LogInformation("User {Username} was not found in Dynamics, will not perform update", username);
                return string.Empty;
            }

            if (entry.IsDisabled.HasValue && entry.IsDisabled.Value)
            {
                _logger.LogInformation("User {SystemUser} is already disabled in Dynamics, will not perform update", new { entry.DomainName, entry.IsDisabled, entry.SystemUserId });
                return string.Empty; // user does not exist, or is already disabled
            }

            await UpdateSystemUserDisableFlag(client, entry.SystemUserId, true);

            return string.Empty;
        }

        public override async Task<bool> UserHasAccessAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            IODataClient client = GetODataClient();

            string logon = IDIR.Logon(username);

            var entry = await GetSystemUserByLogon(client, logon);

            return entry?.IsDisabled != null && !entry.IsDisabled.Value;
        }

        private Task<IDictionary<string, object>> UpdateSystemUserDisableFlag(IODataClient client, Guid systemUserId, bool isDisabled)
        {
            _logger.LogDebug("Updating  SystemUser with {SystemUserId} with isDisabled flag set to {UsDisabled} in Dynamics", systemUserId, isDisabled);

            // using the untyped version because the typed version was giving an error:
            //
            // Microsoft.OData.ODataException: The property 'isdisabled' does not exist on type 'Microsoft.Dynamics.CRM.systemuser'.
            // Make sure to only use property names that are defined by the type or mark the type as open type.
            //
            return client
                .For("systemuser")
                .Key(systemUserId)
                .Set(new { isdisabled = isDisabled })
                .UpdateEntryAsync();
        }

        private async Task<SystemUser> GetSystemUserByLogon(IODataClient client, string logon)
        {
            _logger.LogDebug("Getting SystemUser with {DomainName} from Dynamics", logon);

            var systemUser = await client
                .For<SystemUser>()
                .Filter(_ => _.DomainName == logon)
                .FindEntryAsync();

            if (systemUser != null)
            {
                _logger.LogDebug("Found SystemUser {@SystemUser}", new { systemUser.DomainName, systemUser.IsDisabled, systemUser.SystemUserId });
            }
            else
            {
                _logger.LogDebug("SystemUser with {DomainName} not found", logon);
            }

            return systemUser;
        }

        private async Task<BusinessUnit> GetRootBusinessUnit(IODataClient client)
        {
            _logger.LogDebug("Getting root business unit from dynamics");

            // should be only one, however, get all just in case
            IEnumerable<BusinessUnit> entries = await client
                .For<BusinessUnit>()
                .Filter(_ => _.ParentBusinessUnit == null)
                .FindEntriesAsync();

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
                    _logger.LogInformation("Found duplicate root {@DuplicateBusinessUnit}, the first root {@BusinessUnit} will be used",
                        entry,
                        businessUnit);
                }
            }

            if (businessUnit == null)
            {
                _logger.LogWarning("No root business unit found where ParentBusinessUnit is null");
            }

            return businessUnit;
        }

        private IODataClient GetODataClient() => _factory.Create(Project.Id + "-dynamics");
    }
}
