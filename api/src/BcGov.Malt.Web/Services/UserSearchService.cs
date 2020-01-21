using BcGov.Malt.Web.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BcGov.Malt.Web.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserSearchService
    {
        /// <summary>
        /// 
        /// </summary>
        Task<User> SearchAsync(string query);
    }

    /// <summary>
    /// 
    /// </summary>
    public class UserSearchService : IUserSearchService
    {
        private static readonly User[] _users = new []
        {
            new User { UserName = "pbolduc", Id = Guid.NewGuid().ToString("d") },
            new User { UserName = "tclausen", Id = Guid.NewGuid().ToString("d") },
            new User { UserName = "choban", Id = Guid.NewGuid().ToString("d") },
            new User { UserName = "nyang", Id = Guid.NewGuid().ToString("d") },
            new User { UserName = "ckelso", Id = Guid.NewGuid().ToString("d") },
            new User { UserName = "sdevalapurkar", Id = Guid.NewGuid().ToString("d") }
        };

        /// <summary>
        /// 
        /// </summary>
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
