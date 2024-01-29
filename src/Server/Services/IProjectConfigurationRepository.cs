using BcGov.Jag.AccountManagement.Server.Models.Configuration;

namespace BcGov.Jag.AccountManagement.Server.Services;

/// <summary>
/// Provides access to the configured list of project configurations.
/// </summary>
public interface IProjectConfigurationRepository
{
    /// <summary>
    /// Gets the list of project configurtions
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<List<ProjectConfiguration>> GetProjectConfigurationsAsync(CancellationToken cancellationToken);
}


public class ConfigurationSystemProjectConfigurationRepository : IProjectConfigurationRepository
{
    private readonly IConfiguration _configuration;

    public ConfigurationSystemProjectConfigurationRepository(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public Task<List<ProjectConfiguration>> GetProjectConfigurationsAsync(CancellationToken cancellationToken)
    {
        List<ProjectConfiguration> projects = _configuration.GetProjectConfigurations(Serilog.Log.Logger)
            .OrderBy(_ => _.Name)
            .ToList();

        return Task.FromResult(projects);
    }
}
