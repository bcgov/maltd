using BcGov.Jag.AccountManagement.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;

namespace BcGov.Jag.AccountManagement.Client.Data;

public static class Extensions
{
    public static WebAssemblyHostBuilder UseData(this WebAssemblyHostBuilder builder)
    {
        var baseAddress = builder.HostEnvironment.BaseAddress;

        // only send the access token on requests to the API
        builder.Services
            .AddRefitClient<IUserApi>()
            .ConfigureHttpClient(c => { c.BaseAddress = new Uri(baseAddress); })
            .AddHttpMessageHandler(services =>
                services.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(new[] { baseAddress + "api/" }));

        // register the repository
        builder.Services.AddTransient<IRepository, Repository>();

        return builder;
    }

    public static IEnumerable<ProjectMembershipModel> GetChanges(this DetailedUser user, IList<ProjectMembershipModel> otherProjects)
    {
        // assumption: both data sets represent the same set of projects

        var projects = ToViewModel(user);

        foreach (var project in projects)
        {
            var otherProject = otherProjects.Single(_ => _.ProjectName == project.ProjectName);

            var membership = new ProjectMembershipModel { ProjectName = project.ProjectName };
            if (project.Dynamics != null && project.Dynamics != otherProject.Dynamics) membership.Dynamics = otherProject.Dynamics;
            if (project.SharePoint != null && project.SharePoint != otherProject.SharePoint) membership.SharePoint = otherProject.SharePoint;

            yield return membership;
        }
    }

    public static IList<ProjectMembershipModel> ToViewModel(this DetailedUser user)
    {
        if (user.Projects is not null && user.Projects.Length > 0)
        {
            var rows = user.Projects.Select(project =>
            {
                return new ProjectMembershipModel
                {
                    ProjectName = project.Name,
                    Dynamics = IsMember(project?.Resources, "Dynamics"),
                    SharePoint = IsMember(project?.Resources, "SharePoint"),
                };
            })
            .ToList();

            return rows;
        }

        return Array.Empty<ProjectMembershipModel>();
    }

    private static bool? IsMember(List<ProjectResourceStatus>? resources, string type)
    {
        var resourceStatus = resources?.FirstOrDefault(_ => _.Type == type);
        if (resourceStatus is null)
        {
            return null;
        }

        return resourceStatus.Status == "member";
    }
}
