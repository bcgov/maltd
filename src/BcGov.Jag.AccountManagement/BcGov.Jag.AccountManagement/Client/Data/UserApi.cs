using BcGov.Jag.AccountManagement.Shared;
using Refit;

namespace BcGov.Jag.AccountManagement.Client.Data;

public interface IUserApi
{
    [Get("/api/user?q={username}")]
    Task<User> SearchAsync(string username);

    [Get("/api/user/{username}")]
    Task<DetailedUser> LookupAsync(string username);

    [Post("/api/user/UpdateUserProjects/{username}")]
    Task UpdateUserProjectsAsync(string username, IList<ProjectMembershipModel> projectMembership);
}
