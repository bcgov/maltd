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
        void Add(string user);

        /// <summary>
        /// Removes the specified user.
        /// </summary>
        void Remove(string user);
    }
}
