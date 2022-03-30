using System.Net;
using System.Net.Http.Headers;
using System.Text;
using BcGov.Jag.AccountManagement.Server.Infrastructure;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Models.SharePoint;
using BcGov.Jag.AccountManagement.Server.Services.Sharepoint;
using Refit;
using ILogger = Serilog.ILogger;

namespace BcGov.Jag.AccountManagement.Server.Services;

public interface ISharePointClient
{
    [Get("/_api/Web?$select=Title,ServerRelativeUrl")]
    public Task<GetSharePointWebVerboseResponse> GetWebAsync(CancellationToken cancellationToken);

    [Get("/_api/Web/SiteGroups?$select=Id,Title")]
    public Task<GetSiteGroupsVerboseResponse> GetSiteGroupsAsync(CancellationToken cancellationToken);

    [Get("/_api/Web/SiteGroups/GetById({siteGroupId})/Users")]
    public Task<string> GetSiteGroupUsersAsync(int siteGroupId);

    [Get("/_api/Web/SiteGroups?$filter=Title%20eq%20'{title}'")]
    public Task<GetSiteGroupsVerboseResponse> GetSiteGroupsByTitleAsync(string title, CancellationToken cancellationToken);

    [Get("/_api/Web/GetUserById({userId})/Groups")]
    public Task<GetSiteGroupsVerboseResponse> GetUserGroupsAsync(int userId, CancellationToken cancellationToken);

    /////// <remarks>Current version of Refit always encodes the loginName making this not working</remarks>>
    ////[Get("/_api/Web/SiteGroups/GetById({siteGroupId})/Users?$filter=LoginName eq '{loginName}'")]
    ////public Task<GetSiteUsersVerboseResponse> GetUserInGroupByLoginNameAsync(int siteGroupId, string loginName);

    [Get("/_api/Web/SiteGroups/GetById({siteGroupId})/Users")]
    public Task<GetSiteUsersVerboseResponse> GetUsersInGroupAsync(int siteGroupId, CancellationToken cancellationToken);

    [Post("/_api/Web/SiteGroups({siteGroupId})/Users/RemoveById({userId})")]
    public Task RemoveUserFromSiteGroupAsync(int siteGroupId, int userId, CancellationToken cancellationToken);

    [Post("/_api/ContextInfo")]
    public Task<GetContextWebInformationVerboseResponse> GetContextWebInformationAsync(CancellationToken cancellationToken);

    [Post("/_api/Web/SiteGroups({siteGroupId})/Users")]
    public Task AddUserToGroupAsync(int siteGroupId, LoginUser user, CancellationToken cancellationToken);
}

public class SharePointResourceUserManagementService : ResourceUserManagementService
{
    private static readonly StringComparer LoginNameComparer = StringComparer.OrdinalIgnoreCase;

    private readonly RefitSettings _refitSettings = new RefitSettings();

    private readonly IUserSearchService _userSearchService;
    private readonly ISamlAuthenticator _samlAuthenticator;

    public SharePointResourceUserManagementService(
        ProjectConfiguration project,
        ProjectResource projectResource,
        IUserSearchService userSearchService,
        ISamlAuthenticator samlAuthenticator,
        ILogger logger)
        : base(project, projectResource, logger)
    {
        _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
        _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
    }

    public override async Task<string> AddUserAsync(Shared.User user, CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Add User To Project");
        activity?.AddTag("project.name", Project.Name);
        activity?.AddTag("project.type", "SharePoint");

        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(user));
        }

        if (string.IsNullOrEmpty(user?.UserPrincipalName))
        {
            Logger.Information("Cannot locate UPN for for {Username}, cannot add users access", user.UserName);
            return $"Unable to locate user's User Principal Name (UPN)";
        }

        ISharePointClient restClient = await GetSharePointRestClientForUpdate(cancellationToken);

        GetSharePointWebVerboseResponse web = await restClient.GetWebAsync(cancellationToken);

        if (string.IsNullOrEmpty(web?.Data?.Title))
        {
            Logger.Warning("Cannot get site group name for {Project} on resource {ResourceType}", Project.Name, ProjectResource.Type);
            return $"Unable to get SharePoint site group name";
        }

        // we always add users to '<site-group> Members'
        var siteGroupTitle = web.Data.Title + " Members";

        GetSiteGroupsVerboseResponse siteGroups =
            await restClient.GetSiteGroupsByTitleAsync(siteGroupTitle, cancellationToken);

        if (siteGroups.Data.Results.Count == 0)
        {
            Logger.Information("Cannot find site group {@SiteGroup}", new SiteGroup { Title = siteGroupTitle });
            return $"SharePoint site group '{siteGroupTitle}' not found";
        }

        var siteGroup = siteGroups.Data.Results[0];

        Logger.Information("Adding {Username} to site collection {@SiteGroup} for {Project} on resource {ResourceType}",
            user.UserName,
            siteGroup,
            Project.Name,
            ProjectResource.Type);


        string logonName = Constants.LoginNamePrefix + user.UserPrincipalName;

        try
        {
            // be sure to use the LoginUser object to ensure we do not send null or default values when adding
            await restClient.AddUserToGroupAsync(siteGroup.Id, new LoginUser { LoginName = logonName }, cancellationToken);
            return string.Empty;
        }
        catch (ApiException e)
        {
            var errorResponse = await e.GetContentAsAsync<SharePointErrorResponse>();
            Logger.Warning(e, "Error adding user to SharePoint group {@Error}", errorResponse);
            return $"Error occurred adding user to SharePoint site group '{siteGroupTitle}'";
        }
    }
    
    public override async Task<string> RemoveUserAsync(Shared.User user, CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Remove User From Project");
        activity?.AddTag("project.name", Project.Name);
        activity?.AddTag("project.type", "SharePoint");

        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(user));
        }

        Logger.Debug("Removing access for {Username}", user.UserName);

        if (string.IsNullOrEmpty(user.UserPrincipalName))
        {
            Logger.Information("Cannot locate UPN for for {Username}, cannot remove users access", user.UserName);
            return "User not found";
        }
        
        // format the SharePoint login name format
        string loginName = Constants.LoginNamePrefix + user.UserPrincipalName;

        ISharePointClient restClient = await GetSharePointRestClientForUpdate(cancellationToken);

        var groups = await restClient.GetSiteGroupsAsync(cancellationToken);
        var siteGroups = groups.Data.Results;

        StringBuilder response = new StringBuilder();

        foreach (var siteGroup in siteGroups)
        {
            try
            {
                var getUsersResponse = await restClient.GetUsersInGroupAsync(siteGroup.Id, cancellationToken);

                var users = getUsersResponse.Data.Results.Where(_ => LoginNameComparer.Equals(_.LoginName, loginName));

                foreach (var sharePointUser in users)
                {
                    Logger.Information("Removing {@User} from site group {@SiteGroup}", sharePointUser, siteGroup);

                    try
                    {
                        await restClient.RemoveUserFromSiteGroupAsync(siteGroup.Id, sharePointUser.Id, cancellationToken);
                    }
                    catch (ApiException e)
                    {
                        var errorResponse = await e.GetContentAsAsync<SharePointErrorResponse>();
                        Logger.Warning(e, "Error removing user from SharePoint group {@Error}", errorResponse);

                        response.Append(response.Length != 0 ? ", " : "Error removing user from site group(s): ");
                        response.Append(siteGroup.Title);
                    }
                }
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.Forbidden)
            {
                // we dont have access to all site groups
                Logger.Debug(e, "No access to {@SiteGroup}, unable to remove {Username} access", siteGroup, user.UserName);
            }
        }

        return response.ToString();
    }

    public override async Task<bool> UserHasAccessAsync(Shared.User user, CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Check User Access");
        activity?.AddTag("project.name", Project.Name);
        activity?.AddTag("project.type", "SharePoint");

        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrEmpty(user.UserName))
        {
            throw new ArgumentException("Username cannot be null or empty", nameof(user));
        }

        if (string.IsNullOrEmpty(user.UserPrincipalName))
        {
            Logger.Information("User does not have a UPN, cannot check users access");
            return false;
        }

        using var userHasAccessActivity = Diagnostics.Source.StartActivity("Check User Has Access");
        userHasAccessActivity?.AddTag("project.name", this.Project.Name);
        userHasAccessActivity?.AddTag("project.type", "SharePoint");

        Logger.Debug("Checking if user has access to project");

        // format the SharePoint login name format
        string loginName = Constants.LoginNamePrefix + user.UserPrincipalName;

        ISharePointClient restClient = await GetSharePointRestClient();

        var groups = await restClient.GetSiteGroupsAsync(cancellationToken);
        var siteGroups = groups.Data.Results;

        // service account does not have permission to view membership of "Excel Services Viewers"
        // TODO: make this configurable
        foreach (var siteGroup in siteGroups)
        {
            try
            {
                var getUsersResponse = await restClient.GetUsersInGroupAsync(siteGroup.Id, cancellationToken);
                var groupMember = getUsersResponse.Data.Results.Any(_ => LoginNameComparer.Equals(_.LoginName, loginName));

                if (groupMember)
                {
                    Logger.Debug("{@User} has access because they are in site group {@SiteGroup}", user, siteGroup);
                    return true;
                }
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.Forbidden)
            {
                // we dont have access to all site groups
                Logger.Debug(e, "No access to {@SiteGroup}, unable to check access", siteGroup);
            }
        }

        return false;
    }

    public override async Task<IList<UserStatus>> GetUsersAsync(CancellationToken cancellationToken)
    {
        using var activity = Diagnostics.Source.StartActivity("Get SharePoint Users");

        Logger.Information("Getting user list from SharePoint is not currently supported. Returning empty list");
        return Array.Empty<UserStatus>();
    }

    private async Task<ISharePointClient> GetSharePointRestClient()
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        var httpClient = await GetHttpClientAsync();
#pragma warning restore CA2000 // Dispose objects before losing scope

        return RestService.For<ISharePointClient>(httpClient, _refitSettings);
    }

    private async Task<ISharePointClient> GetSharePointRestClientForUpdate(CancellationToken cancellationToken)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        var httpClient = await GetHttpClientAsync();
#pragma warning restore CA2000 // Dispose objects before losing scope
        var restClient = RestService.For<ISharePointClient>(httpClient, _refitSettings);

        var contextWebInformationResponse = await restClient.GetContextWebInformationAsync(cancellationToken);

        httpClient.DefaultRequestHeaders.Add("X-RequestDigest", contextWebInformationResponse.Data.ContextWebInformation.FormDigestValue);

        return restClient;
    }

    private async Task<HttpClient> GetHttpClientAsync()
    {
        var cookieContainer = new CookieContainer();
#pragma warning disable CA2000 // Dispose objects before losing scope
        HttpMessageHandler handler = new SocketsHttpHandler
        {
            UseCookies = true,
            AllowAutoRedirect = false,
            CookieContainer = cookieContainer,
            MaxConnectionsPerServer = 25
        };

        if (!string.IsNullOrEmpty(ProjectResource.ApiGatewayHost) && !string.IsNullOrEmpty(ProjectResource.ApiGatewayPolicy))
        {
            // since this is executed on every access to sharepoint, only log at debug level
            Logger.Debug("Using {@ApiGateway} for {Resource}", 
                new { Host = ProjectResource.ApiGatewayHost, Policy = ProjectResource.ApiGatewayPolicy }, 
                ProjectResource.Resource);
            
            handler = new ApiGatewayHandler(handler, ProjectResource.ApiGatewayHost, ProjectResource.ApiGatewayPolicy);
        }
#pragma warning restore CA2000 // Dispose objects before losing scope

        HttpClient httpClient = new HttpClient(handler);
        httpClient.BaseAddress = ProjectResource.Resource;
        httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose"));

        // simplify the parameters
        var resource = ProjectResource.Resource;
        var relyingPartyIdentifier = ProjectResource.RelyingPartyIdentifier;
        var username = ProjectResource.Username;
        var password = ProjectResource.Password;
        var authorizationUrl = ProjectResource.AuthorizationUri.ToString();

        string samlToken = await _samlAuthenticator.GetStsSamlTokenAsync(relyingPartyIdentifier, username, password, authorizationUrl);

        string apiGatewayHost = ProjectResource.ApiGatewayHost;
        string apiGatewayPolicy = ProjectResource.ApiGatewayPolicy;

        await _samlAuthenticator.GetSharepointFedAuthCookieAsync(resource, samlToken, httpClient, cookieContainer, apiGatewayHost, apiGatewayPolicy);

        return httpClient;
    }
}
