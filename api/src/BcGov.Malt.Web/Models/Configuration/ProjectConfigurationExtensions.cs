using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Models.Configuration
{
    public static class ProjectConfigurationExtensions
    {
        public static IEnumerable<ProjectConfiguration> GetProjectConfigurations(this IConfiguration configuration, Serilog.ILogger logger)
        {
            // it is ok if this is null or empty if we do not want to use the API Gateway
            string apiGatewayHost = configuration["ApiGatewayHost"];

            if (string.IsNullOrEmpty(apiGatewayHost))
            {
                logger.Information("ApiGatewayHost is not configured, API Gateway will not be used.");
            }
            else
            {
                logger.Information("ApiGatewayHost is configured as {ApiGatewayHost}, API Gateway will be used on those resources with a ApiGatewayPolicy.", apiGatewayHost);
            }

            HashSet<string> uniqueProjectNames = new HashSet<string>();

            // we require to read the configuration to get the list of 
            // projects so we can register everything up 
            var projectsSection = configuration.GetSection("Projects");

            int index = 0;
            foreach (var project in projectsSection.GetChildren())
            {
                var projectConfiguration = project.Get<ProjectConfiguration>();

                if (IsValid(apiGatewayHost, projectConfiguration, index, logger))
                {
                    if (!uniqueProjectNames.Contains(projectConfiguration.Name))
                    {
                        uniqueProjectNames.Add(projectConfiguration.Name);
                        yield return projectConfiguration;
                    }
                    else
                    {
                        Serilog.Log.Logger.Error("Duplicate project {Name} at {Index}, project names must be unique", projectConfiguration.Name, index);
                    }
                }

                index++;
            }
        }

        private static bool IsValid(string apiGatewayHost, ProjectConfiguration projectConfiguration, int index, Serilog.ILogger logger)
        {
            List<string> errors = new List<string>();

            string projectName = $"Project configuration index {index}";

            // TODO: created validator?
            if (string.IsNullOrEmpty(projectConfiguration.Name))
            {
                errors.Add($"Project name is null or empty at configuration index {index}");
            }
            else
            {
                projectName = projectConfiguration.Name;
            }

            if (projectConfiguration.Resources == null || projectConfiguration.Resources.Count == 0)
            {
                errors.Add("No project resources defined");
            }
            else
            {
                for (int i = 0; i < projectConfiguration.Resources.Count; i++)
                {
                    ProjectResource projectResource = projectConfiguration.Resources[i];
                    if (projectResource.Resource == null)
                    {
                        errors.Add($"Project resource url is null at index {i}");
                    }
                    else
                    {
                        ApplyApiGatewayPolicy(apiGatewayHost, projectResource, logger);
                    }

                    if (!Enum.IsDefined(typeof(ProjectType), projectResource.Type))
                    {
                        errors.Add($"Project resource type is invalid at index {i}");
                    }

                    if (projectResource.Type == ProjectType.Dynamics && string.IsNullOrEmpty(projectResource.ClientId))
                    {
                        errors.Add($"Dynamics Project resource ClientId is missing at index {i}");
                    }

                    if (projectResource.Type == ProjectType.Dynamics && string.IsNullOrEmpty(projectResource.ClientSecret))
                    {
                        errors.Add($"Dynamics Project resource ClientSecret is missing at index {i}");
                    }

                    if (projectResource.Type == ProjectType.SharePoint && string.IsNullOrEmpty(projectResource.RelyingPartyIdentifier))
                    {
                        errors.Add($"SharePoint Project resource RelyingPartyIdentifier is missing at index {i}");
                    }

                    if (string.IsNullOrEmpty(projectResource.Username))
                    {
                        errors.Add($"Project resource Username is missing at index {i}");
                    }

                    if (string.IsNullOrEmpty(projectResource.Password))
                    {
                        errors.Add($"Project resource Resource Password is missing at index {i}");
                    }
                }
            }

            if (errors.Count != 0)
            {
                Serilog.Log.Logger
                    .ForContext("Project", new { Name = projectName, Index = index })
                    .ForContext("Errors", errors)
                    .Error("Project configuration has errors and will be skipped");
            }

            return errors.Count == 0;
        }

        private static void ApplyApiGatewayPolicy(string apiGatewayHost, ProjectResource projectResource, Serilog.ILogger logger)
        {
            if (string.IsNullOrEmpty(apiGatewayHost) || string.IsNullOrEmpty(projectResource.ApiGatewayPolicy))
            {
                projectResource.BaseAddress = projectResource.Resource;
            }
            else
            {
                // https://<host>/policy
                UriBuilder builder = new UriBuilder(projectResource.Resource);
                builder.Host = apiGatewayHost;

                // inject the policy name as the first uri segment
                builder.Path = builder.Path.StartsWith("/", StringComparison.InvariantCulture)
                    ? "/" + projectResource.ApiGatewayPolicy + builder.Path
                    : "/" + projectResource.ApiGatewayPolicy + "/" + builder.Path;

                projectResource.BaseAddress = builder.Uri;
            }

            logger.Information("Using {BaseAddress} for {Resource}", projectResource.BaseAddress, projectResource.Resource);
        }
    }
}
