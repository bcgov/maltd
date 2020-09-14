using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Infrastructure;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Models.SharePoint;
using BcGov.Malt.Web.Services.Sharepoint;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Refit;

namespace BcGov.Malt.Web.Services
{
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
        public Task AddUserToGroupAsync(int siteGroupId, User user, CancellationToken cancellationToken);
    }

    public class SharePointResourceUserManagementService : ResourceUserManagementService
    {
        private static readonly StringComparer LoginNameComparer = StringComparer.OrdinalIgnoreCase;

        private readonly ILogger<SharePointResourceUserManagementService> _logger;

        private readonly RefitSettings _refitSettings = new RefitSettings
        {
            ContentSerializer = new SystemTextJsonContentSerializer(new JsonSerializerOptions())
        };

        private readonly IUserSearchService _userSearchService;
        private readonly ISamlAuthenticator _samlAuthenticator;

        public SharePointResourceUserManagementService(
            ProjectConfiguration project,
            ProjectResource projectResource,
            IUserSearchService userSearchService,
            ISamlAuthenticator samlAuthenticator,
            ILogger<SharePointResourceUserManagementService> logger)
            : base(project, projectResource, logger)
        {
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
            _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<string> AddUserAsync(string username, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            var user = await _userSearchService.SearchAsync(username);

            if (string.IsNullOrEmpty(user?.UserPrincipalName))
            {
                _logger.LogInformation("Cannot locate UPN for for {Username}, cannot add users access", username);
                return $"Unable to locate user's User Principal Name (UPN)";
            }

            ISharePointClient restClient = await GetSharePointRestClientForUpdate(cancellationToken);

            GetSharePointWebVerboseResponse web = await restClient.GetWebAsync(cancellationToken);

            if (string.IsNullOrEmpty(web?.Data?.Title))
            {
                _logger.LogWarning("Cannot get site group name for {Project} on resource {ResourceType}", Project.Name, ProjectResource.Type);
                return $"Unable to get SharePoint site group name";
            }

            // we always add users to '<site-group> Members'
            var siteGroupTitle = web.Data.Title + " Members";

            GetSiteGroupsVerboseResponse siteGroups = await restClient.GetSiteGroupsByTitleAsync(siteGroupTitle, cancellationToken);

            if (siteGroups.Data.Results.Count == 0)
            {
                _logger.LogInformation("Cannot find site group {@SiteGroup}", new SiteGroup { Title = siteGroupTitle });
                return $"SharePoint site group '{siteGroupTitle}' not found";
            }

            var siteGroup = siteGroups.Data.Results[0];

            _logger.LogInformation("Adding {Username} to site collection {@SiteGroup} for {Project} on resource {ResourceType}",
                username,
                siteGroup,
                Project.Name,
                ProjectResource.Type);


            string logonName = Constants.LoginNamePrefix + user.UserPrincipalName;

            try
            {
                await restClient.AddUserToGroupAsync(siteGroup.Id, new User { LoginName = logonName }, cancellationToken);
                return string.Empty;
            }
            catch (ApiException e)
            {
                var errorResponse = await e.GetContentAsAsync<SharePointErrorResponse>();
                _logger.LogWarning(e, "Error adding user to Sharepoint group {@Error}", errorResponse);
                return $"Error occuring adding user to SharePoint site group '{siteGroupTitle}'";
            }
        }
        
        public override async Task<string> RemoveUserAsync(string username, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            _logger.LogDebug("Removing access for {Username}", username);

            var user = await _userSearchService.SearchAsync(username);

            if (string.IsNullOrEmpty(user?.UserPrincipalName))
            {
                _logger.LogInformation("Cannot locate UPN for for {Username}, cannot remove users access", username);
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
                        _logger.LogInformation("Removing {@User} from site group {@SiteGroup}", sharePointUser, siteGroup);

                        try
                        {
                            await restClient.RemoveUserFromSiteGroupAsync(siteGroup.Id, sharePointUser.Id, cancellationToken);
                        }
                        catch (ApiException e)
                        {
                            var errorResponse = await e.GetContentAsAsync<SharePointErrorResponse>();
                            _logger.LogWarning(e, "Error removing user from SharePoint group {@Error}", errorResponse);

                            response.Append(response.Length != 0 ? ", " : "Error removing user from site group(s): ");
                            response.Append(siteGroup.Title);
                        }
                    }
                }
                catch (ApiException e) when (e.StatusCode == HttpStatusCode.Forbidden)
                {
                    // we dont have access to all site groups
                    _logger.LogDebug(e, "No access to {@SiteGroup}, unable to remove {Username} access", siteGroup, username);
                }
            }

            return response.ToString();
        }

        public override async Task<bool> UserHasAccessAsync(string username, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            }

            var user = await _userSearchService.SearchAsync(username);

            if (string.IsNullOrEmpty(user?.UserPrincipalName))
            {
                _logger.LogInformation("Cannot locate UPN for for {Username}, cannot check users access", username);
                return false;
            }

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
                        _logger.LogDebug("{@User} has access because they are in site group {@SiteGroup}", user, siteGroup);
                        return true;
                    }
                }
                catch (ApiException e) when (e.StatusCode == HttpStatusCode.Forbidden)
                {
                    // we dont have access to all site groups
                    _logger.LogDebug(e, "No access to {@SiteGroup}, unable to check access", siteGroup);
                }
            }

            return false;
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
            HttpMessageHandler handler = new HttpClientHandler
            {
                UseCookies = true,
                AllowAutoRedirect = false,
                CookieContainer = cookieContainer
            };

            if (!string.IsNullOrEmpty(ProjectResource.ApiGatewayHost) && !string.IsNullOrEmpty(ProjectResource.ApiGatewayPolicy))
            {
                // since this is executed on every access to sharepoint, only log at debug level
                _logger.LogDebug("Using {@ApiGateway} for {Resource}", 
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

            await _samlAuthenticator.GetSharepointFedAuthCookieAsync(resource, samlToken, httpClient, cookieContainer);

            return httpClient;
        }
    }


    public sealed class SystemTextJsonContentSerializer : IContentSerializer
    {
        private static readonly MediaTypeHeaderValue ContentType = new MediaTypeHeaderValue("application/json")
        {
            CharSet = Encoding.UTF8.WebName
        };

        public SystemTextJsonContentSerializer(JsonSerializerOptions serializerOptions)
        {
            SerializerOptions = serializerOptions;
        }

        private JsonSerializerOptions SerializerOptions { get; }

        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            using var utf8Json = await content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<T>(utf8Json, SerializerOptions);
            return data;
        }

        public Task<HttpContent> SerializeAsync<T>(T item)
        {
            StringContent content = null;

            try
            {
                // this was using a memory stream but it was failing to serialize on the request
                string json = JsonSerializer.Serialize(item, SerializerOptions);
#pragma warning disable CA2000 // Call System.IDisposable.Dispose, justification: value is being returned
                content = new StringContent(json, Encoding.UTF8, "application/json");
#pragma warning restore CA2000

                content.Headers.ContentType = ContentType;

                HttpContent httpContent = content;
                return Task.FromResult(httpContent);
            }
            catch (Exception)
            {
                content?.Dispose();
                throw;
            }
        }
    }
}
