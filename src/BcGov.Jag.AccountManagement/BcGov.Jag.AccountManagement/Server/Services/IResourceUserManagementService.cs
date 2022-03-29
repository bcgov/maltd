using BcGov.Jag.AccountManagement.Shared;

namespace BcGov.Jag.AccountManagement.Server.Services;

/// <summary>
/// Provides an abstraction for managing users in a specific kind of resource.
/// </summary>
public interface IResourceUserManagementService
{
    /// <summary>
    /// Adds the specified user.
    /// </summary>
    Task<string> AddUserAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the specified user.
    /// </summary>
    Task<string> RemoveUserAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the specified user has access.
    /// </summary>
    Task<bool> UserHasAccessAsync(User user, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a list of users from the resource.
    /// </summary>
    Task<IList<UserStatus>> GetUsersAsync(CancellationToken cancellationToken);
}
