namespace BcGov.Jag.AccountManagement.Server.Models.Configuration;

public static class ProjectConfigurationExtensions
{
    public static IEnumerable<ProjectConfiguration> GetProjectConfigurations(this IConfiguration configuration, Serilog.ILogger logger)
    {
        // it is ok if this is null or empty if we do not want to use the API Gateway
        string apiGatewayHost = configuration["ApiGatewayHost"];

        if (!string.IsNullOrEmpty(apiGatewayHost))
        {
            logger.Warning("Deprecation Warning: ApiGatewayHost is configured at the root level with value {ApiGatewayHost}, set the ApiGatewayHost on each resource.", apiGatewayHost);
        }

        HashSet<string> uniqueProjectNames = new HashSet<string>();

        // we require to read the configuration to get the list of 
        // projects so we can register everything up 
        var projectsSection = configuration.GetSection("Projects");

        int index = 0;
        foreach (var project in projectsSection.GetChildren())
        {
            var projectConfiguration = project.Get<ProjectConfiguration>();

            if (IsValid(projectConfiguration, index, logger))
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

    private static bool IsValid(ProjectConfiguration projectConfiguration, int index, Serilog.ILogger logger)
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
            logger
                .ForContext("Project", new { Name = projectName, Index = index })
                .ForContext("Errors", errors)
                .Error("Project configuration has errors and will be skipped");
        }

        return errors.Count == 0;
    }
}
