using System.Threading.Tasks;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Models.Dynamics;
using Simple.OData.Client;

namespace BcGov.Malt.Web.Services
{
    public class DynamicsResourceUserManagementService : ResourceUserManagementService
    {
        private readonly IODataClientFactory _factory;

        public DynamicsResourceUserManagementService(IODataClientFactory factory, ProjectConfiguration project, ProjectResource projectResource) 
            : base(project, projectResource)
        {
            _factory = factory ?? throw new System.ArgumentNullException(nameof(factory));
        }

        public override Task AddUserAsync(string user)
        {
            return Task.CompletedTask;
        }

        public override Task RemoveUserAsync(string user)
        {
            return Task.CompletedTask;
        }

        public override async Task<bool> UserHasAccessAsync(string user)
        {
            string logon = IDIR.Logon(user);

            IODataClient client = _factory.Create(Project.Id + "-dynamics");

            var entry = await client.For<SystemUser>()
                .Filter(_ => _.DomainName == logon)
                .FindEntryAsync();

            return entry != null && entry.IsDisabled.HasValue && !entry.IsDisabled.Value;
        }

    }
}
