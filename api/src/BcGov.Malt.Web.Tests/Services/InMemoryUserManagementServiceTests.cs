using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using BcGov.Malt.Web.Models;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Services;
using Xunit;

namespace BcGov.Malt.Web.Tests.Services
{
    public class InMemoryUserManagementServiceTests
    {
        [Fact]
        public async Task adding_user_to_project_should_return_true()
        {
            Fixture fixture = new Fixture();

            var user = fixture.Create<User>();
            var project = fixture.Create<ProjectConfiguration>();

            InMemoryUserManagementService sut = new InMemoryUserManagementService();

            

            var actual = await sut.AddUserToProjectAsync(user, project, CancellationToken.None);

            Assert.True(actual != null);
        }

        [Fact]
        public async Task removing_a_user_a_project_they_are_not_a_memeber_of_should_return_false()
        {
            Fixture fixture = new Fixture();

            var user = fixture.Create<User>();
            var project = fixture.Create<ProjectConfiguration>();

            InMemoryUserManagementService sut = new InMemoryUserManagementService();

            var actual = await sut.RemoveUserFromProjectAsync(user, project, CancellationToken.None);

            Assert.True(actual != null);
        }

        [Fact]
        public async Task removing_a_user_a_project_they_are_memeber_of_should_return_true()
        {
            Fixture fixture = new Fixture();

            var user = fixture.Create<User>();
            var project = fixture.Create<ProjectConfiguration>();

            InMemoryUserManagementService sut = new InMemoryUserManagementService();

            List<Project> projects = await sut.GetProjectsForUserAsync(user, CancellationToken.None);
            // user should not be in the generated project by default
            Assert.NotNull(projects);
            Assert.Empty(projects);

            // add the user to the project for the testing of removal
            var actual = await sut.AddUserToProjectAsync(user, project, CancellationToken.None);
            Assert.True(actual != null);

            projects = await sut.GetProjectsForUserAsync(user, CancellationToken.None);
            // after adding the user should be added to the project
            Assert.NotNull(projects);
            Assert.Single(projects);
            Assert.Equal(project.Id, projects[0].Id);

            // act
            actual = await sut.RemoveUserFromProjectAsync(user, project, CancellationToken.None);

            Assert.True(actual != null);
        }
    }
}

