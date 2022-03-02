namespace BcGov.Jag.AccountManagement.Server.Services;

/// <summary>
/// Provides an abstraction for managing users in a specific kind of resource.
/// </summary>
public interface IResourceUserManagementService
{
    /// <summary>
    /// Adds the specified user.
    /// </summary>
    Task<string> AddUserAsync(string username, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the specified user.
    /// </summary>
    Task<string> RemoveUserAsync(string username, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the specified user has access.
    /// </summary>
    Task<bool> UserHasAccessAsync(string username, CancellationToken cancellationToken);
}
