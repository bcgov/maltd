﻿using System.Text;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Services;
using BcGov.Jag.AccountManagement.Server.Services.Sharepoint;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BcGov.Jag.AccountManagement.Server.HealthChecks;

public class ProjectAccessTokenHealthCheck : AccessTokenHealthCheckBase
{
    private readonly ProjectConfiguration _project;

    public ProjectAccessTokenHealthCheck(ProjectConfiguration project, ISamlAuthenticator samlAuthenticator, IOAuthClientFactory oauthClientFactory) 
        : base(samlAuthenticator, oauthClientFactory)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
    }

    public override async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        List<Tuple<ProjectResource, Exception>> results = await CheckAccessTokensAsync();
        var failedCount = results.Count(_ => _.Item2 != null);

        // TODO: review how these message look in the health check endpoints
        string description;

        if (failedCount == 0)
        {
            description = $"{results.Count} of {results.Count} access tokens retrieved successfully";

            return HealthCheckResult.Healthy(description);
        }

        if (failedCount == results.Count)
        {
            return HealthCheckResult.Unhealthy($"None of the {failedCount} access tokens retrieved successfully", CreateAggregateException(results));
        }

        StringBuilder descriptionBuilder = new StringBuilder($"Failed to get {failedCount} of {results.Count} access tokens. Resources that failed: ");
        
        bool first = true;
        foreach (var failed in results.Where(_ => _.Item2 != null))
        {
            if (!first)
            {
                descriptionBuilder.Append(", ");
            }

            descriptionBuilder.Append(failed.Item1.Type);
            first = false;
        }

        description = descriptionBuilder.ToString();
        return HealthCheckResult.Degraded(description, CreateAggregateException(results));
    }


    private async Task<List<Tuple<ProjectResource, Exception>>> CheckAccessTokensAsync()
    {
        List<Task<Tuple<ProjectResource, Exception>>> tasks = new List<Task<Tuple<ProjectResource, Exception>>>();

        foreach (ProjectResource resource in _project.Resources)
        {
            tasks.Add(Task.Run(() => CheckAccessTokenAsync(_project, resource)));
        }

        await Task.WhenAll(tasks);

        return tasks.Select(_ => _.Result).ToList();
    }
}
