﻿using BcGov.Jag.AccountManagement.Shared;
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
        var response = await _userApi.LookupAsync(username);
        if (response.IsSuccessStatusCode)
        {
            return response.Content;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            
            return null;
        }

        // TODO: handle other error, HttpStatusCode.InternalServerError
        return null;
    }

    public async Task UpdateUserProjectsAsync(string username, IList<ProjectMembershipModel> projectMembership)
    {
        await _userApi.UpdateUserProjectsAsync(username, projectMembership);
    }

    public async Task validateProjectConfigAsync(ProjectResource projectResource) {

        await _userApi.validateProjectConfigAsync(projectResource);
    }
    
    public async Task<Stream> GetUserAccessReportAsync()
    {
        var response = await _userApi.GetUserAccessReportAsync();
        Stream stream = await response.Content.ReadAsStreamAsync();
        return stream;
    }
}
