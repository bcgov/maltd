using System;
using System.Threading.Tasks;
using AutoFixture;
using BcGov.Malt.Web.Services;
using Xunit;

namespace BcGov.Malt.Web.Tests.Services
{
    public class InMemoryUserSearchServiceTests
    {
        [Fact]
        public async Task search_should_return_null_if_search_for_null()
        {
            InMemoryUserSearchService sut = new InMemoryUserSearchService();

            var actual = await sut.SearchAsync(null);

            Assert.Null(actual);
        }


        [Fact]
        public async Task search_should_return_null_if_search_was_not_found()
        {
            Fixture fixuture = new Fixture();

            string query = fixuture.Create<string>();

            InMemoryUserSearchService sut = new InMemoryUserSearchService();

            var actual = await sut.SearchAsync(query);

            Assert.Null(actual);
        }

        [Fact]
        public async Task search_should_return_the_user_if_search_was_found()
        {
            string expected = "sdevalapurkar"; // hard coded in the service

            InMemoryUserSearchService sut = new InMemoryUserSearchService();

            var actual = await sut.SearchAsync(expected);

            Assert.NotNull(actual);
            Assert.Equal(expected, actual.UserName);
        }
    }
}
