using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models
{
    /// <summary>
    /// Represents a KeyCloak user
    /// </summary>
    public class User
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool? Enabled { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; }

    }
}
