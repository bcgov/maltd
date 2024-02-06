using BcGov.Jag.AccountManagement.Shared;
using FluentResults;

namespace BcGov.Jag.AccountManagement.Client.Data;

/// <summary>
/// Provides wrapper around the various APIs
/// </summary>
public interface IRepository
{ 
    Task<Result<DetailedUser>> LookupAsync(string username);

    Task UpdateUserProjectsAsync(string username, IList<ProjectMembershipModel> projectMembership);

    Task<Stream> GetUserAccessReportAsync();
}
