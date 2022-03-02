using BcGov.Jag.AccountManagement.Shared;
using Refit;
using System.Net;

namespace BcGov.Jag.AccountManagement.Client.Data;

public class Repository : IRepository
{
    private readonly IUserApi _userApi;

    public Repository(IUserApi userApi)
    {
        _userApi = userApi ?? throw new ArgumentNullException(nameof(userApi));
    }

    public async Task<User?> SearchAsync(string username)
    {
        try
        {
            var user = await _userApi.SearchAsync(username);
            return user;
        }
        catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}
