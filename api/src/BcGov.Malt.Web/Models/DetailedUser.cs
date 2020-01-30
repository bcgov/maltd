using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models
{
    /// <summary>
    /// Represents a user with the currently assigned projects
    /// </summary>
    public class DetailedUser : User
    {
        /// <summary>Initializes a new instance of the <see cref="DetailedUser"/> class.</summary>
        /// <param name="user">The user.</param>
        /// <param name="projects">The projects.</param>
        public DetailedUser(User user, IEnumerable<Project> projects)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Email = user.Email;
            UserName = user.UserName;
            Enabled = true;
            Projects = projects != null ? projects.ToArray() : Array.Empty<Project>();
        }

        /// <summary>
        /// The projects the user is assigned to.
        /// </summary>
        [JsonPropertyName("projects")]
        public Project[] Projects { get; set; }
    }
}
