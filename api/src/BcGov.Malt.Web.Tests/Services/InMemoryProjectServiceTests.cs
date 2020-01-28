using System.Threading.Tasks;
using BcGov.Malt.Web.Services;
using Xunit;

namespace BcGov.Malt.Web.Tests.Services
{
    public class InMemoryProjectServiceTests
    {
        [Fact]
        public async Task should_return_a_non_empty_project_collection()
        {
            InMemoryProjectService sut = new InMemoryProjectService();

            var actual = await sut.GetProjectsAsync();
            Assert.NotEmpty(actual);
        }
    }
}
