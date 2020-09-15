﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Services;
using BcGov.Malt.Web.Services.Sharepoint;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BcGov.Malt.Web.HealthChecks
{
    public abstract class AccessTokenHealthCheckBase : IHealthCheck
    {
        private static readonly Exception _noException = null;

        private readonly ISamlAuthenticator _samlAuthenticator;
        private readonly IOAuthClientFactory _oauthClientFactory;

        protected AccessTokenHealthCheckBase(ISamlAuthenticator samlAuthenticator, IOAuthClientFactory oauthClientFactory)
        {
            _samlAuthenticator = samlAuthenticator ?? throw new ArgumentNullException(nameof(samlAuthenticator));
            _oauthClientFactory = oauthClientFactory ?? throw new ArgumentNullException(nameof(oauthClientFactory));
        }

        public abstract Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken());

        protected async Task<Tuple<ProjectResource, Exception>> CheckAccessTokenAsync(ProjectConfiguration project, ProjectResource resource)
        {
            switch (resource.Type)
            {
                case ProjectType.Dynamics:
                    return await CheckOAuthAccessTokenAsync(project, resource).ConfigureAwait(false);

                case ProjectType.SharePoint:
                    return await CheckSamlAccessTokenAsync(project, resource).ConfigureAwait(false);

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
                var oauthClient = _oauthClientFactory.Create(project);

                // IOAuthClient does not cache tokens
                Token token = await oauthClient.GetTokenAsync(options, CancellationToken.None).ConfigureAwait(false);
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
                string token = await _samlAuthenticator.GetStsSamlTokenAsync(relyingParty, username, password, stsUri, cached: false).ConfigureAwait(false);
                return Tuple.Create(resource, _noException);
            }
            catch (Exception e)
            {
                return Tuple.Create(resource, e);
            }
        }

        protected AggregateException CreateAggregateException(List<Tuple<ProjectResource, Exception>> results)
        {
            return new AggregateException(results.Where(_ => _.Item2 != null).Select(_ => _.Item2));
        }
    }
}