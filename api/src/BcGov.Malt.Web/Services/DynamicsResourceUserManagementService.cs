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

        public override async Task AddUserAsync(string username)
        {
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
                entry = await UpdateSystemUserDisableFlag(client, entry.SystemUserId, false);
            }
            else
            {
                _logger.LogInformation("{@SystemUser} exists and is already enabled user", entry);
            }
        }

        public override async Task RemoveUserAsync(string username)
        {
            IODataClient client = GetODataClient();
            string logon = IDIR.Logon(username);

            SystemUser entry = await GetSystemUserByLogon(client, logon);

            if (entry == null || entry.IsDisabled.HasValue && entry.IsDisabled.Value)
            {
                return; // user does not exist, or is already disabled
            }

            entry = await UpdateSystemUserDisableFlag(client, entry.SystemUserId, true);
        }

        public override async Task<bool> UserHasAccessAsync(string username)
        {
            IODataClient client = GetODataClient();

            string logon = IDIR.Logon(username);

            var entry = await GetSystemUserByLogon(client, logon);

            return entry?.IsDisabled != null && !entry.IsDisabled.Value;
        }

        private Task<SystemUser> UpdateSystemUserDisableFlag(IODataClient client, Guid systemUserId, bool isDisabled)
        {
            return client
                .For<SystemUser>()
                .Key(systemUserId)
                .Set(new { IsDisabled = isDisabled })
                .UpdateEntryAsync();
        }

        private Task<SystemUser> GetSystemUserByLogon(IODataClient client, string logon)
        {
            return client
                .For<SystemUser>()
                .Filter(_ => _.DomainName == logon)
                .FindEntryAsync();
        }

        private async Task<BusinessUnit> GetRootBusinessUnit(IODataClient client)
        {
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
                _logger.LogWarning("No root business unit found");
            }

            return businessUnit;
        }

        private IODataClient GetODataClient() => _factory.Create(Project.Id + "-dynamics");
 
    }
}
