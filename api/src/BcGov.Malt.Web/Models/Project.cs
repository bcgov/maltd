using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models
{
    /// <summary>
    /// Represents a project that a user can be granted access to.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// The id of the project
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// The name of the project
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// The type of project, ie Dynamics or SharePoint
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}
