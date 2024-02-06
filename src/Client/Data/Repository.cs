using BcGov.Jag.AccountManagement.Shared;
using FluentResults;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net;

namespace BcGov.Jag.AccountManagement.Client.Data;

public class Repository : IRepository
{
    private readonly IUserApi _userApi;

    public Repository(IUserApi userApi)
    {
        _userApi = userApi ?? throw new ArgumentNullException(nameof(userApi));
    }

    public async Task<Result<DetailedUser>> LookupAsync(string username)
    {
        try
        {
            var response = await _userApi.LookupAsync(username).ConfigureAwait(false);
            if (response.IsSuccessStatusCode && response.Content is not null)
            {
                return response.Content;
            }

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound: return Result.Fail("User Not Found");
                case HttpStatusCode.Unauthorized: return Result.Fail("Not authorized");
                default:
                    return Result.Fail(response.StatusCode.ToString());
            }
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return Result.Fail(exception.Message);
        }
        catch (Exception exception)
        {
            return Result.Fail($"Error: {exception.Message}");
        }
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
