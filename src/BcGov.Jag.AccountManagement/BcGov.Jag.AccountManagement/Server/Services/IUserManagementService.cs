using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Shared;

namespace BcGov.Jag.AccountManagement.Server.Services;

/// <summary>
/// Provides the ability to add or remove users from projects
/// </summary>
public interface IUserManagementService
{
    /// <summary>
    /// Adds a user to a project. The user will be added to each of the defined project resources, ie Dynamics and SharePoint
    /// </summary>
    /// <param name="user">The user to change</param>
    /// <param name="project">The project to add the user to</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns <c>true</c> if the user was added to the project, otherwise <c>false</c></returns>
    Task<List<ProjectResourceStatus>> AddUserToProjectAsync(User user, ProjectConfiguration project, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a user from a project.
    /// </summary>
    /// <param name="user">The user to change</param>
    /// <param name="project">The project to remove the user from.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns <c>true</c> if the user was removed from the project, otherwise <c>false</c></returns>
    Task<List<ProjectResourceStatus>> RemoveUserFromProjectAsync(User user, ProjectConfiguration project, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all the projects a user is assigned to.
    /// </summary>
    /// <param name="user">The user to get projects for.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The list of projects a user is currently assigned to.</returns>
    Task<List<Project>> GetProjectsForUserAsync(User user, CancellationToken cancellationToken);

    Task ChangeUserProjectAccessAsync(User user, ProjectConfiguration project, ProjectMembershipModel projectMembership, CancellationToken cancellationToken);

    Task<IList<UserStatus>> GetUsersAsync(ProjectConfiguration project, ProjectResource resource, CancellationToken cancellationToken);
}
