using BcGov.Jag.AccountManagement.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
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

    public async Task<DetailedUser?> LookupAsync(string username)
    {
        try
        {
            var response = await _userApi.LookupAsync(username).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return response.Content;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {

                return null;
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        // TODO: handle other error, HttpStatusCode.InternalServerError
        return null;
    }

    public async Task UpdateUserProjectsAsync(string username, IList<ProjectMembershipModel> projectMembership)
    {
        try
        {
            await _userApi.UpdateUserProjectsAsync(username, projectMembership).ConfigureAwait(false);
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
    }

    public async Task<Stream> GetUserAccessReportAsync()
    {
        try
        {
            var response = await _userApi.GetUserAccessReportAsync().ConfigureAwait(false);
            Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return stream;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null!; // dont think this really gets executed
        }
    }
}
