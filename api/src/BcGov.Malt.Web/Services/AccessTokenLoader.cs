using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.HealthChecks;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Services.Sharepoint;

namespace BcGov.Malt.Web.Services
{
    public class AccessTokenLoader
    {
        private static readonly Exception _noException = null;

        private readonly ProjectConfigurationCollection _projects;
        private readonly ITokenService _tokenService;
        private readonly ISamlAuthenticator _samlAuthenticator;

        public AccessTokenLoader(ProjectConfigurationCollection projects, ITokenService tokenService, ISamlAuthenticator samlAuthenticator)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
        }

        public async Task<List<Tuple<ProjectResource, Exception>>> GetAccessTokensAsync()
        {
            List<Task<Tuple<ProjectResource, Exception>>> tasks = new List<Task<Tuple<ProjectResource, Exception>>>();

            foreach (var projectConfiguration in _projects)
            {
                foreach (ProjectResource resource in projectConfiguration.Resources)
                {
                    tasks.Add(GetAccessTokenAsync(resource));
                }
            }

            await Task.WhenAll(tasks);

            return tasks.Select(_ => _.Result).ToList();
        }


        private async Task<Tuple<ProjectResource, Exception>> GetAccessTokenAsync(ProjectResource resource)
        {
            switch (resource.Type)
            {
                case ProjectType.Dynamics:
                    return await GetOAuthAccessTokenAsync(resource);

                case ProjectType.SharePoint:
                    return await GetSamlAccessTokenAsync(resource);

                default:
                    Exception exception = new InvalidProjectTypeException(resource.Type);
                    return Tuple.Create(resource, exception);
            }
        }


        private async Task<Tuple<ProjectResource, Exception>> GetOAuthAccessTokenAsync(ProjectResource resource)
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
                // IOAuthClient does not cache tokens
                var token = await _tokenService.GetTokenAsync(options, CancellationToken.None);
                return Tuple.Create(resource, _noException);
            }
            catch (Exception e)
            {
                return Tuple.Create(resource, e);
            }
        }

        private async Task<Tuple<ProjectResource, Exception>> GetSamlAccessTokenAsync(ProjectResource resource)
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
