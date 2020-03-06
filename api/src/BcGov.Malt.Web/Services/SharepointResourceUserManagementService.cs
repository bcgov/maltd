using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    public class SharepointResourceUserManagementService : ResourceUserManagementService
    {
        public SharepointResourceUserManagementService(ProjectConfiguration project, ProjectResource projectResource) 
            : base(project, projectResource)
        {
        }

        public override Task AddUserAsync(string user)
        {
            return Task.CompletedTask;
        }

        public override Task RemoveUserAsync(string user)
        {
            return Task.CompletedTask;
        }

        public override Task<bool> UserHasAccessAsync(string user)
        {
            return Task.FromResult(false);
        }
    }
}
