using BcGov.Jag.AccountManagement.Shared;

namespace BcGov.Jag.AccountManagement.Client.Data;

/// <summary>
/// Provides wrapper around the various APIs
/// </summary>
public interface IRepository
{
    Task<User?> SearchAsync(string username);
}
