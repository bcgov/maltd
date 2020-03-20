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
        Task<string> AddUserAsync(string username);

        /// <summary>
        /// Removes the specified user.
        /// </summary>
        Task<string> RemoveUserAsync(string username);

        /// <summary>
        /// Determines whether the specified user has access.
        /// </summary>
        /// <param name="username">The user.</param>
        Task<bool> UserHasAccessAsync(string username);
    }
}
