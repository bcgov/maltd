using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models;

namespace BcGov.Malt.Web.Services
{

    /// <summary>
    /// A mock, in memory user management service.
    /// </summary>
    /// <seealso cref="BcGov.Malt.Web.Services.IUserManagementService" />
    public class InMemoryUserManagementService : IUserManagementService
    {
        private readonly Dictionary<User, List<Project>> _database = new Dictionary<User, List<Project>>();

        /// <summary>
        /// Adds a user to a project.
        /// </summary>
        /// <param name="user">The user to change</param>
        /// <param name="project">The project to add the user to</param>
        /// <returns>Returns <c>true</c> if the user was added to the project, otherwise <c>false</c></returns>
        public Task<List<ProjectResourceStatus>> AddUserToProjectAsync(User user, Project project)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (project == null) throw new ArgumentNullException(nameof(project));

            if (!_database.TryGetValue(user, out List<Project> projects))
            {
                projects = new List<Project>();
                _database.Add(user, projects);
            }

            if (projects.All(_ => _.Id != project.Id))
            {
                projects.Add(project);
            }
            
            var statuses = project.Resources
                .Select(_ => new ProjectResourceStatus {Type = _.Type, Status = ProjectResourceStatuses.Member})
                .ToList();

            return Task.FromResult(statuses);
        }

        /// <summary>
        /// Removes a user from a project.
        /// </summary>
        /// <param name="user">The user to change</param>
        /// <param name="project">The project to remove the user from.</param>
        /// <returns>Returns <c>true</c> if the user was removed from the project, otherwise <c>false</c></returns>
        public Task<List<ProjectResourceStatus>> RemoveUserFromProjectAsync(User user, Project project)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (project == null) throw new ArgumentNullException(nameof(project));

            if (!_database.TryGetValue(user, out List<Project> projects))
            {
                projects = new List<Project>();
                _database.Add(user, projects);
            }

            var statuses = project.Resources
                .Select(_ => new ProjectResourceStatus { Type = _.Type, Status = ProjectResourceStatuses.NotMember })
                .ToList();

            return Task.FromResult(statuses);
        }


        /// <summary>
        /// Gets all the projects a user is assigned to.
        /// </summary>
        /// <param name="user">The user to get projects for.</param>
        /// <returns>The list of projects a user is currently assigned to.</returns>
        public Task<List<Project>> GetProjectsForUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (!_database.TryGetValue(user, out List<Project> projects))
            {
                projects = new List<Project>();
                _database.Add(user, projects);
            }

            return Task.FromResult(projects);
        }
    }
}
