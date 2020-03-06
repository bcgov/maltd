using System.Threading.Tasks;

namespace BcGov.Malt.Web.Services
{
    /// <summary>
    /// Provides an abstraction for managing users in a specific kind of resource.
    /// </summary>
    public interface IResourceUserManagementService
    {
        /// <summary>
        /// Adds the specified user.
        /// </summary>
        Task AddUserAsync(string user);

        /// <summary>
        /// Removes the specified user.
        /// </summary>
        Task RemoveUserAsync(string user);

        /// <summary>
        /// Determines whether the specified user has access.
        /// </summary>
        /// <param name="user">The user.</param>
        Task<bool> UserHasAccessAsync(string user);
    }
}
