using System.Threading.Tasks;
using BcGov.Malt.Web.Models;

namespace BcGov.Malt.Web.Services
{
    /// <summary>
    /// An interface for searching for users.
    /// </summary>
    public interface IUserSearchService
    {
        /// <summary>Searches for a user asynchronously.</summary>
        /// <param name="query">The username to query for</param>
        /// <returns>The found user or null if not found.</returns>
        Task<User> SearchAsync(string query);
    }
}
