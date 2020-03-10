using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        Task<int> AddUser(String SiteGroupName, String UserNameToAdd);
        Task<int> RemoveUser(String SiteGroupName, String UserNameToRemove);
        Task<bool> IsUserAMemberAsync(String SiteCollectionName, String UserName);
    }

    public class SharePointHttpClientService : ISharePointHttpClientService
    {
         IUserSearchService _userSearchService;
        HttpClient httpClient;
        private readonly ILogger<SharePointHttpClientService> _logger;

        private readonly ProjectResource _resource;

        public SharePointHttpClientService(ProjectResource projectResource, IUserSearchService userSearchService, ILogger<SharePointHttpClientService> logger)
        {
            _resource = projectResource;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            httpClient = GetHttpClient(_resource);
            _userSearchService = userSearchService;
        }

        private HttpClient GetHttpClient(ProjectResource resource)
        {
            CookieContainer _cookieContainer = new CookieContainer();
            HttpClientHandler httpClientHandler = new HttpClientHandler() { UseCookies = true, AllowAutoRedirect = false, CookieContainer = _cookieContainer };
            HttpClient httpClient = new HttpClient(httpClientHandler);
            httpClient.BaseAddress = resource.Resource;
            httpClient.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose"));

            string samlToken = Authentication.GetStsSamlToken(resource.RelyingPartyIdentifier, resource.Username, resource.Password, "https://ststest.gov.bc.ca/adfs/services/trust/2005/UsernameMixed" /*resource.AuthorizationUri*/)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            Authentication.GetSharepointFedAuthCookie(resource.Resource, samlToken, httpClient, _cookieContainer)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
       
            return httpClient;
        }


        private async Task<string> OnGetFormDigest()
        {
            HttpResponseMessage response = await httpClient.PostAsync("_api/contextinfo", null);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<ContextWebInformation>(content);
                return result.ResponseData.GetContextWebInformation.FormDigestValue;
            }
            else
            {
                _logger.LogError("Request to generate Form Digest value failed ");
                return null;
            }
        }

        private async Task<int> GetSiteGroupId(String siteName)
        {

            HttpResponseMessage response = await httpClient.GetAsync($"_api/web/sitegroups/");
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var SiteCollectionResult = JsonSerializer.Deserialize<SiteCollection>(content);

                List<SiteCollectionResult> value = SiteCollectionResult.SiteData.SiteResult.Where(_ => _.LoginName.Contains(Constants.SiteGroupNames.Members)).ToList();

                if (value.Count == 0)
                {
                    _logger.LogError("No site group Members is  found ");
                    return -1;
                }
                else

                    return value[0].Id;
            }
            else
            {
                _logger.LogError("Request failed to get all the site groups ");
                return -1;
            }
        }

        private async Task<int> GetUserID(int SiteCollectionId, String UserName)
        {
            HttpResponseMessage response = await httpClient.GetAsync($"_api/web/sitegroups/getbyid({SiteCollectionId})/users");
            if (response.IsSuccessStatusCode)
            {

                string resultFromResponse = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<SharepointUser>(resultFromResponse);
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                List<SharepointUserResult> value = result.Userdata.results
                    .Where(_ => comparer.Equals(_.LoginName, UserName))
                    .ToList();

                if (value.Count == 0)
                {
                    _logger.LogError("No User Id is found ");
                    return -1;
                }
                else
                    return value[0].Id;
            }
            else
            {
                _logger.LogError("Request failed to query the users " + response );
                return -1;
            }

        }

        public async Task<int> AddUser(String SiteGroupName, String UserNameToAdd)
        {
            _logger.LogDebug($"Adding {UserNameToAdd} to Site colection Group {SiteGroupName}");

            string SiteName = SiteGroupName;

            var UserName = await _userSearchService.GetUserPrincipalNameAsync(UserNameToAdd);

            int siteId = await GetSiteGroupId(SiteName);
            string XRequestDigest = await OnGetFormDigest();


            httpClient.DefaultRequestHeaders.Add("X-RequestDigest", XRequestDigest);

            AddUserBody body = SetBody(UserName);

            HttpResponseMessage response = await httpClient.PostODataJsonAsync($"_api/web/sitegroups({siteId})/users", body);

            string resultFromResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<AddUserResponse>(resultFromResponse);
                return (int)response.StatusCode;
            }

            return (int)response.StatusCode;
        }

        public async Task<int> RemoveUser(String SiteGroupName, String UserNameToRemove)
        {
            _logger.LogDebug($"Removing {UserNameToRemove} from Site colection Group {SiteGroupName}");

            string SiteName = SiteGroupName;
            var UserName = await _userSearchService.GetUserPrincipalNameAsync(UserNameToRemove);
            UserName = Constants.UserOperations.Claims + UserName;
            int siteId = await GetSiteGroupId(SiteName);
            int userId = await GetUserID(siteId, UserName);

            string XRequestDigest = await OnGetFormDigest();

            httpClient.DefaultRequestHeaders.Add("X-RequestDigest", XRequestDigest);
          
            HttpResponseMessage response = await httpClient.PostAsync($"_api/web/sitegroups({siteId})/users/removebyid({userId})", null);

            string resultFromResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RemoveUserResponse>(resultFromResponse);
                return (int)response.StatusCode; ;
            }

            return (int)response.StatusCode; ;
        }

        private AddUserBody SetBody(string UserNameToAdd)
        {
            AddUserBody body = new AddUserBody();
            body.AddUserBodymetadata = new AddUserBodyMetadata();
            body.AddUserBodymetadata.type = Constants.UserOperations.UserType; //"SP.User";
            body.LoginName = Constants.UserOperations.Claims + UserNameToAdd;
            return body;

        }

        public async Task<bool> IsUserAMemberAsync(String SiteCollectionName, String Username)
        {
            List<string> result = new List<string>();

            var username = await _userSearchService.GetUserPrincipalNameAsync(Username);

            username = Constants.UserOperations.Claims + username;

            int siteId = await GetSiteGroupId(SiteCollectionName);

            HttpResponseMessage response = await httpClient.GetAsync($"_api/web/sitegroups/getbyid({siteId})/users");
            string resultFromResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var results = JsonSerializer.Deserialize<SiteCollection>(resultFromResponse);
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;

                bool isMember = results.SiteData.SiteResult.Any(_ => comparer.Equals(_.LoginName, username));
                return isMember;
            }
            return false;
        }
    }

    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> PostODataJsonAsync<TBody>(this HttpClient client, string requestUri, TBody body)
        {
            string json = JsonSerializer.Serialize(body);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose");

            return client.PostAsync(requestUri, content);
        }
    }
}
