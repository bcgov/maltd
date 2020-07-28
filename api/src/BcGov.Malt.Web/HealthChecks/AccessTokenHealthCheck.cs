using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Services;
using BcGov.Malt.Web.Services.Sharepoint;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BcGov.Malt.Web.HealthChecks
{
    public class AccessTokenHealthCheck : IHealthCheck
    {
        private static readonly Exception _noException = null;

        private readonly ProjectConfigurationCollection _projects;

        private readonly IOAuthClient _oauthClient;
        private readonly ISamlAuthenticator _samlAuthenticator;

        public AccessTokenHealthCheck(ProjectConfigurationCollection projects, IOAuthClient oauthClient, ISamlAuthenticator samlAuthenticator)
        {
            _projects = projects ?? throw new ArgumentNullException(nameof(projects));
            _oauthClient = oauthClient ?? throw new ArgumentNullException(nameof(oauthClient));
            _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
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

            foreach (var projectConfiguration in _projects)
            {
                foreach (ProjectResource resource in projectConfiguration.Resources)
                {
                    tasks.Add(CheckAccessTokenAsync(resource));
                }
            }

            await Task.WhenAll(tasks);

            return tasks.Select(_ => _.Result).ToList();
        }

        private async Task<Tuple<ProjectResource, Exception>> CheckAccessTokenAsync(ProjectResource resource)
        {
            switch (resource.Type)
            {
                case ProjectType.Dynamics:
                    return await CheckOAuthAccessTokenAsync(resource);
 
                case ProjectType.SharePoint:
                    return await CheckSamlAccessTokenAsync(resource);

                default:
                    Exception exception = new InvalidProjectTypeException(resource.Type);
                    return Tuple.Create(resource, exception);
            }
        }

        private async Task<Tuple<ProjectResource, Exception>> CheckOAuthAccessTokenAsync(ProjectResource resource)
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
                var token = await _oauthClient.GetTokenAsync(options, CancellationToken.None);
                return Tuple.Create(resource, _noException);
            }
            catch (Exception e)
            {
                return Tuple.Create(resource, e);
            }
        }

        private async Task<Tuple<ProjectResource, Exception>> CheckSamlAccessTokenAsync(ProjectResource resource)
        {
            string relyingParty = resource.RelyingPartyIdentifier;
            string username = resource.Username;
            string password = resource.Password;
            string stsUri = resource.AuthorizationUri.ToString();

            try
            {
                // dont cache the tokens
                var token = await _samlAuthenticator.GetStsSamlTokenAsync(relyingParty, username, password, stsUri, cached: false);
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
