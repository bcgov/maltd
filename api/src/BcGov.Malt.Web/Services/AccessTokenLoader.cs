using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.HealthChecks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Services.Sharepoint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Services
{
    public interface IAccessTokenLoader
    {
        Task<List<Tuple<ProjectResource, Exception>>> GetAccessTokensAsync();
    }

    public class AccessTokenLoader : IAccessTokenLoader
    {
        private static readonly Exception _noException = null;

        private readonly ProjectConfigurationCollection _projects;
        private readonly ISamlAuthenticator _samlAuthenticator;
        private readonly IServiceProvider _serviceProvider;

        public AccessTokenLoader(ProjectConfigurationCollection projects, ISamlAuthenticator samlAuthenticator, IServiceProvider serviceProvider)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<List<Tuple<ProjectResource, Exception>>> GetAccessTokensAsync()
        {
            List<Task<Tuple<ProjectResource, Exception>>> tasks = new List<Task<Tuple<ProjectResource, Exception>>>();

            foreach (var project in _projects)
            {
                foreach (ProjectResource resource in project.Resources)
                {
                    tasks.Add(GetAccessTokenAsync(project, resource));
                }
            }

            await Task.WhenAll(tasks);

            return tasks.Select(_ => _.Result).ToList();
        }


        private async Task<Tuple<ProjectResource, Exception>> GetAccessTokenAsync(ProjectConfiguration project, ProjectResource resource)
        {
            switch (resource.Type)
            {
                case ProjectType.Dynamics:
                    return await GetOAuthAccessTokenAsync(project, resource);

                case ProjectType.SharePoint:
                    return await GetSamlAccessTokenAsync(project, resource);

                default:
                    Exception exception = new InvalidProjectTypeException(resource.Type);
                    return Tuple.Create(resource, exception);
            }
        }


        private async Task<Tuple<ProjectResource, Exception>> GetOAuthAccessTokenAsync(ProjectConfiguration project, ProjectResource resource)
        {

            var options = new OAuthOptions
            {
                AuthorizationUri = resource.AuthorizationUri,
                ClientId = resource.ClientId,
                ClientSecret = resource.ClientSecret,
                Username = resource.Username,
                Password = resource.Password,
                Resource = resource.Resource
            };

            try
            {
                // OAuth is always dynamics
                string projectResourceKey = project.Id + "-dynamics";

                var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
                HttpClient httpClient = httpClientFactory.CreateClient(projectResourceKey + "-authorization");

                // create the handler that will authenticate the call and add authorization header
                ILogger<OAuthClient> clientLogger = _serviceProvider.GetRequiredService<ILogger<OAuthClient>>();
                var tokenCache = _serviceProvider.GetRequiredService<ITokenCache<OAuthOptions, Token>>();
                ITokenService tokenService = new OAuthTokenService(new OAuthClient(httpClient, clientLogger), tokenCache);

                // IOAuthClient does not cache tokens
                var token = await tokenService.GetTokenAsync(options, CancellationToken.None);
                return Tuple.Create(resource, _noException);
            }
            catch (Exception e)
            {
                return Tuple.Create(resource, e);
            }
        }

        private async Task<Tuple<ProjectResource, Exception>> GetSamlAccessTokenAsync(ProjectConfiguration project, ProjectResource resource)
        {
            string relyingParty = resource.RelyingPartyIdentifier;
            string username = resource.Username;
            string password = resource.Password;
            string stsUri = resource.AuthorizationUri.ToString();

            try
            {
                // dont cache the tokens
                var token = await _samlAuthenticator.GetStsSamlTokenAsync(relyingParty, username, password, stsUri, cached: true);
                return Tuple.Create(resource, _noException);
            }
            catch (Exception e)
            {
                return Tuple.Create(resource, e);
            }
        }

    }
}
