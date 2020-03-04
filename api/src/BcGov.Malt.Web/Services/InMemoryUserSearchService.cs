using BcGov.Malt.Web.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BcGov.Malt.Web.Services
{
    /// <summary>Represents a class that searches from a hard coded list of users.</summary>
    /// <seealso cref="BcGov.Malt.Web.Services.IUserSearchService" />
    public class InMemoryUserSearchService : IUserSearchService
    {
        private readonly User[] _users = new []
        {
            new User { UserName = "pbolduc", Id = Guid.NewGuid().ToString("d", CultureInfo.InvariantCulture),FirstName= "Phil",LastName= "Bolduc",Email = "phil.bolduc@example.com" },
            new User { UserName = "tclausen", Id = Guid.NewGuid().ToString("d", CultureInfo.InvariantCulture),FirstName= "Taylor", LastName= "Clausen",Email = "taylor.clausen@example.org" },
            new User { UserName = "choban", Id = Guid.NewGuid().ToString("d", CultureInfo.InvariantCulture),FirstName= "Chris",LastName= "Hoban",Email = "chris.hoban@example.net" },
            new User { UserName = "nyang", Id = Guid.NewGuid().ToString("d", CultureInfo.InvariantCulture),FirstName= "Nan",LastName= "Yang",Email = "nan.yang@example.ca" },
            new User { UserName = "ckelso", Id = Guid.NewGuid().ToString("d", CultureInfo.InvariantCulture),FirstName= "Charlotte",LastName= "Kelso", Email = "charlotte.kelso@example.com" },
            new User { UserName = "sdevalapurkar", Id = Guid.NewGuid().ToString("d", CultureInfo.InvariantCulture),FirstName= "Shreyas",LastName="Devalapurkar",Email = "shreyas.devalapurkar@example" },

        };

        /// <summary>Searches for a user asynchronously.</summary>
        /// <param name="query">The username to query for</param>
        /// <returns>The found user or null if not found.</returns>
        public async Task<User> SearchAsync(string query)
        {
            User user = null;

            if (!string.IsNullOrEmpty(query))
            {
                user = _users.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Equals(_.UserName, query));
            }

            if (user == null)
            {
                return null;
            }

            await Task.Delay(0); // to suppress warning CS1998: This async method lacks 'await'

            return user;
        }
    }
}
