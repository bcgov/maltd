using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Services;
using BcGov.Malt.Web.Services.Sharepoint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.HealthChecks
{
    public class AccessTokenHealthCheck : IHealthCheck
    {
        private static readonly Exception _noException = null;

        private readonly ProjectConfigurationCollection _projects;
        private readonly ISamlAuthenticator _samlAuthenticator;
        private readonly IServiceProvider _serviceProvider;

        public AccessTokenHealthCheck(ProjectConfigurationCollection projects, ISamlAuthenticator samlAuthenticator, IServiceProvider serviceProvider)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            List<Tuple<ProjectResource, Exception>> results = await CheckAccessTokensAsync();

            var failedCount = results.Count(_ => _.Item2 != null);

            if (failedCount == 0)
            {
                return HealthCheckResult.Healthy($"All {results.Count} access tokens retrieved successfully");
            }

            if (failedCount == results.Count)
            {
                return HealthCheckResult.Unhealthy("No access tokens retrieved successfully", CreateAggregateException(results));
            }

            return HealthCheckResult.Degraded($"{failedCount} access tokens failed", CreateAggregateException(results));

        }

        private AggregateException CreateAggregateException(List<Tuple<ProjectResource, Exception>> results)
        {
            return new AggregateException(results.Where(_ => _.Item2 != null).Select(_ => _.Item2));
        }

        private async Task<List<Tuple<ProjectResource, Exception>>> CheckAccessTokensAsync()
        {
            List<Task<Tuple<ProjectResource, Exception>>> tasks = new List<Task<Tuple<ProjectResource, Exception>>>();

            foreach (ProjectConfiguration project in _projects)
            {
                foreach (ProjectResource resource in project.Resources)
                {
                    tasks.Add(CheckAccessTokenAsync(project, resource));
                }
            }

            await Task.WhenAll(tasks);

            return tasks.Select(_ => _.Result).ToList();
        }

        private async Task<Tuple<ProjectResource, Exception>> CheckAccessTokenAsync(ProjectConfiguration project, ProjectResource resource)
        {
            switch (resource.Type)
            {
                case ProjectType.Dynamics:
                    return await CheckOAuthAccessTokenAsync(project, resource);
 
                case ProjectType.SharePoint:
                    return await CheckSamlAccessTokenAsync(project, resource);

                default:
                    Exception exception = new InvalidProjectTypeException(resource.Type);
                    return Tuple.Create(resource, exception);
            }
        }

        private async Task<Tuple<ProjectResource, Exception>> CheckOAuthAccessTokenAsync(ProjectConfiguration project, ProjectResource resource)
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
                var oauthClient = new OAuthClient(httpClient, clientLogger);

                // IOAuthClient does not cache tokens
                Token token = await oauthClient.GetTokenAsync(options, CancellationToken.None);
                return Tuple.Create(resource, _noException);
            }
            catch (Exception e)
            {
                return Tuple.Create(resource, e);
            }
        }

        private async Task<Tuple<ProjectResource, Exception>> CheckSamlAccessTokenAsync(ProjectConfiguration project, ProjectResource resource)
        {
            string relyingParty = resource.RelyingPartyIdentifier;
            string username = resource.Username;
            string password = resource.Password;
            string stsUri = resource.AuthorizationUri.ToString();

            try
            {
                // dont cache the tokens
                string token = await _samlAuthenticator.GetStsSamlTokenAsync(relyingParty, username, password, stsUri, cached: false);
                return Tuple.Create(resource, _noException);
            }
            catch (Exception e)
            {
                return Tuple.Create(resource, e);
            }
        }
    }

    [Serializable]
    public class InvalidProjectTypeException : Exception
    {
        public ProjectType ProjectType { get; }

        public InvalidProjectTypeException(ProjectType projectType)
        {
            ProjectType = projectType;
        }

        protected InvalidProjectTypeException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
