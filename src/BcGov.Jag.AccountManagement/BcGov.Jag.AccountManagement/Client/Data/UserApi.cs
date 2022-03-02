using BcGov.Jag.AccountManagement.Shared;
using Refit;

namespace BcGov.Jag.AccountManagement.Client.Data;

public interface IUserApi
{
    [Get("/api/user?q={username}")]
    Task<User> SearchAsync(string username);
}
