using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models
{
    /// <summary>
    /// Represents a user with the currently assigned projects
    /// </summary>
    public class DetailedUser : User
    {
        /// <summary>
        /// The projects the user is assigned to.
        /// </summary>
        [JsonPropertyName("projects")]
        public Project[] Projects { get; set; }
    }
}
