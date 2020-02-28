using BcGov.Malt.Web.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static BcGov.Malt.Web.Startup;

namespace BcGov.Malt.Web.Services
{
    public static class ProjectExtensions
    {
        /// <summary>
        /// Configures access to OData services and projects based on configuration.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        public static void AddProjectAccess(this IServiceCollection services, IConfiguration configuration)
        {
            // read the configuration and register the types for each 
            List<ProjectConfiguration> projects = configuration.GetProjectConfigurations()
                .OrderBy(_ => _.Name)
                .ToList();

            // Register the single instance of the ProjectConfigurationCollection for access by other services
            services.AddSingleton(typeof(ProjectConfigurationCollection), new ProjectConfigurationCollection(projects));

            foreach (var project in projects)
            {
                // process each resource in this project
                foreach (var projectResource in project.Resources.Where(_ => _.Type == ProjectType.Dynamics))
                {
                    string projectResourceKey = project.Id + "-dynamics";

                    // add authorization HttpClient 
                    services.AddHttpClient(projectResourceKey + "-authorization",
                        configure => configure.BaseAddress = projectResource.AuthorizationUri)
                        ;

                    // add odata HttpClient 
                    // note: I do not like this IoC anti-pattern where we are using the service locator directly, however,
                    //       there are many named dependencies. There may be an opportunity to address this in the future
                    services.AddHttpClient(projectResourceKey, configure => configure.BaseAddress = projectResource.Resource)
                        .AddHttpMessageHandler((serviceProvider) =>
                        {
                            // build the token service that talk to the OAuth endpoint 
                            IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                            HttpClient httpClient = httpClientFactory.CreateClient(projectResourceKey + "-authorization");

                            // create the handler that will authenticate the call and add authorization header
                            ITokenCache tokenCache = serviceProvider.GetRequiredService<ITokenCache>();
                            ITokenService tokenService = new OAuthTokenService(new OAuthClient(httpClient), tokenCache);
                            var handler = new TokenAuthorizationHandler(tokenService, CreateOAuthOptions(projectResource));
                            return handler;
                        });
                }
            }
        }

        private static OAuthOptions CreateOAuthOptions(ProjectResource projectResource)
        {
            var options = new OAuthOptions
            {
                AuthorizationUri = projectResource.AuthorizationUri,
                Resource = projectResource.Resource,
                Username = projectResource.Username,
                Password = projectResource.Password,
                ClientId = projectResource.ClientId,
                ClientSecret = projectResource.ClientSecret
            };

            return options;
        }
    }
}
