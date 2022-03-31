using BcGov.Jag.AccountManagement.Shared;

namespace BcGov.Jag.AccountManagement.Server.Services;

/// <summary>
/// An interface for searching for users.
/// </summary>
public interface IUserSearchService
{
    /// <summary>Searches for a user.</summary>
    /// <param name="samAccountName">The username to query for</param>
    /// <returns>The found user or null if not found.</returns>
    Task<User?> SearchAsync(string samAccountName, CancellationToken cancellationToken);

    Task<ActiveDirectoryUserStatus?> GetAccountStatusAsync(string username, CancellationToken cancellationToken);
}
