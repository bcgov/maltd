using BcGov.Jag.AccountManagement.Shared;
using System.Globalization;

namespace BcGov.Jag.AccountManagement.Server.Services;

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
    public Task<User?> SearchAsync(string query, CancellationToken cancellationToken)
    {
        User? user = null;

        if (!string.IsNullOrEmpty(query))
        {
            user = _users.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Equals(_.UserName, query));
        }

        if (user == null)
        {
            // ReSharper disable once ExpressionIsAlwaysNull
            return Task.FromResult(user);
        }

        return Task.FromResult(user);
    }

    public Task<string> GetUserPrincipalNameAsync(string samAccountName)
    {
        User? user = null;

        if (!string.IsNullOrEmpty(samAccountName))
        {
            user = _users.FirstOrDefault(_ => StringComparer.OrdinalIgnoreCase.Equals(_.UserName, samAccountName));
        }

        if (user == null)
        {
            string nullString = null;
            return Task.FromResult(nullString);
        }

        return Task.FromResult(user.Email); // UPN is not the same as Email, but good enough for testing
    }

    public Task<ActiveDirectoryUserStatus?> GetAccountStatusAsync(string username, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
