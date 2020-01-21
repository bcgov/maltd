using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models
{
    /// <summary>
    /// Represents a user with the currently assigned projects
    /// </summary>
    public class DetailedUser : User
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("projects")]
        public string[] Projects { get; set; }

    }
}
