using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Models.SharePoint;
using BcGov.Malt.Web.Services.Sharepoint;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Services
{
    public interface ISharePointHttpClientService
    {
        Task<int> AddUser(string siteGroupName, string userNameToAdd);
        Task<int> RemoveUser(string siteGroupName, string userNameToRemove);
        Task<bool> IsUserAMemberAsync(string siteCollectionName, string userName);
    }

    public class SharePointHttpClientService : ISharePointHttpClientService
    {
        private readonly ILogger<SharePointHttpClientService> _logger;
        private readonly ProjectResource _resource;
        private readonly IUserSearchService _userSearchService;

        public SharePointHttpClientService(ProjectResource projectResource, IUserSearchService userSearchService, ILogger<SharePointHttpClientService> logger)
        {
            _resource = projectResource ?? throw new ArgumentNullException(nameof(projectResource));
            _userSearchService = userSearchService ?? throw new ArgumentNullException(nameof(userSearchService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> AddUser(string siteGroupName, string userNameToAdd)
        {
            _logger.LogDebug("Adding {UserNameToAdd} to site collection {SiteGroupName}", userNameToAdd, siteGroupName);

            string siteName = siteGroupName;

            var userName = await _userSearchService.GetUserPrincipalNameAsync(userNameToAdd);

            int siteId = await GetSiteGroupId(siteName);
            string xRequestDigest = await OnGetFormDigest();

            using var httpClient = await GetHttpClientAsync(_resource);
            httpClient.DefaultRequestHeaders.Add("X-RequestDigest", xRequestDigest);

            HttpResponseMessage response = await httpClient.PostODataJsonAsync($"_api/web/sitegroups({siteId})/users", new AddUserRequest(userName));

            if (response.IsSuccessStatusCode)
            {
                // result is not used?
                string content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AddUserResponse>(content);
                return (int)response.StatusCode;
            }

            return (int)response.StatusCode;
        }

        public async Task<int> RemoveUser(string siteGroupName, string userNameToRemove)
        {
            _logger.LogDebug("Removing {UserNameToRemove} from Site collection Group {SiteGroupName}", userNameToRemove, siteGroupName);

            string siteName = siteGroupName;
            var userName = await _userSearchService.GetUserPrincipalNameAsync(userNameToRemove);
            userName = Constants.UserOperations.Claims + userName;
            int siteId = await GetSiteGroupId(siteName);
            int userId = await GetUserId(siteId, userName);

            string xRequestDigest = await OnGetFormDigest();

            using var httpClient = await GetHttpClientAsync(_resource);
            httpClient.DefaultRequestHeaders.Add("X-RequestDigest", xRequestDigest);

            HttpResponseMessage response = await httpClient.PostAsync($"_api/web/sitegroups({siteId})/users/removebyid({userId})", null);

            string resultFromResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // result is not used?
                var result = JsonSerializer.Deserialize<RemoveUserResponse>(resultFromResponse);
                return (int)response.StatusCode;
            }

            return (int)response.StatusCode;
        }

        public async Task<bool> IsUserAMemberAsync(string siteCollectionName, string username)
        {
            var upn = await _userSearchService.GetUserPrincipalNameAsync(username);
            upn = Constants.UserOperations.Claims + upn;

            int siteId = await GetSiteGroupId(siteCollectionName);

            using var httpClient = await GetHttpClientAsync(_resource);
            HttpResponseMessage response = await httpClient.GetAsync($"_api/web/sitegroups/getbyid({siteId})/users");
            string resultFromResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var results = JsonSerializer.Deserialize<SiteCollection>(resultFromResponse);
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;

                bool isMember = results.Data.Results.Any(_ => comparer.Equals(_.LoginName, upn));
                return isMember;
            }
            return false;
        }

        private async Task<HttpClient> GetHttpClientAsync(ProjectResource resource)
        {
            CookieContainer cookieContainer = new CookieContainer();
            using HttpClientHandler httpClientHandler = new HttpClientHandler { UseCookies = true, AllowAutoRedirect = false, CookieContainer = cookieContainer };
            using HttpClient httpClient = new HttpClient(httpClientHandler) { BaseAddress = resource.Resource };

            httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose"));

            string samlToken = await Authentication.GetStsSamlToken(resource.RelyingPartyIdentifier, resource.Username, resource.Password, resource.AuthorizationUri.ToString());

            await Authentication.GetSharepointFedAuthCookie(resource.Resource, samlToken, httpClient, cookieContainer);

            return httpClient;
        }


        private async Task<string> OnGetFormDigest()
        {
            using var httpClient = await GetHttpClientAsync(_resource);

            HttpResponseMessage response = await httpClient.PostAsync("_api/contextinfo", null);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ContextWebInformation>(content);
                return result.ResponseData.GetContextWebInformation.FormDigestValue;
            }

            _logger.LogError("Request to generate Form Digest value failed ");
            return null;
        }

        private async Task<int> GetSiteGroupId(string siteName)
        {
            using var httpClient = await GetHttpClientAsync(_resource);

            HttpResponseMessage response = await httpClient.GetAsync("_api/web/sitegroups/");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var siteCollectionResult = JsonSerializer.Deserialize<SiteCollection>(content);

                List<SiteCollectionResult> value = siteCollectionResult.Data.Results.Where(_ => _.LoginName.Contains(Constants.SiteGroupNames.Members)).ToList();

                if (value.Count == 0)
                {
                    _logger.LogError("No site group Members is  found ");
                    return -1;
                }

                return value[0].Id;
            }

            _logger.LogError("Request failed to get all the site groups ");
            return -1;
        }

        private async Task<int> GetUserId(int siteCollectionId, string userName)
        {
            using var httpClient = await GetHttpClientAsync(_resource);

            HttpResponseMessage response = await httpClient.GetAsync($"_api/web/sitegroups/getbyid({siteCollectionId})/users");
            if (response.IsSuccessStatusCode)
            {
                string resultFromResponse = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<SharepointUser>(resultFromResponse);
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                List<SharepointUserResult> value = result.Data.Results
                    .Where(_ => comparer.Equals(_.LoginName, userName))
                    .ToList();

                if (value.Count == 0)
                {
                    _logger.LogError("No User Id is found ");
                    return -1;
                }

                return value[0].Id;
            }

            _logger.LogError("Request failed to query the users " + response);
            return -1;
        }
    }
}
