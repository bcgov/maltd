using BcGov.Jag.AccountManagement.Shared;
using Refit;

namespace BcGov.Jag.AccountManagement.Client.Data;

public interface IUserApi
{
    [Get("/api/user?q={username}")]
    Task<IApiResponse<User>> SearchAsync(string username);

    [Get("/api/user/{username}")]
    Task<IApiResponse<DetailedUser>> LookupAsync(string username);

    [Post("/api/user/UpdateUserProjects/{username}")]
    Task UpdateUserProjectsAsync(string username, IList<ProjectMembershipModel> projectMembership);
}
