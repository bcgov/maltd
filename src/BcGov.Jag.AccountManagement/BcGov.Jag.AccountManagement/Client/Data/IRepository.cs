using BcGov.Jag.AccountManagement.Shared;

namespace BcGov.Jag.AccountManagement.Client.Data;

/// <summary>
/// Provides wrapper around the various APIs
/// </summary>
public interface IRepository
{ 
    Task<DetailedUser?> LookupAsync(string username);

    Task UpdateUserProjectsAsync(string username, IList<ProjectMembershipModel> projectMembership);
}
