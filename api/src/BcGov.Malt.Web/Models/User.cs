using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models
{
    /// <summary>
    /// Represents a KeyCloak user
    /// </summary>
    public class User
    {
        /// <summary>Gets or sets the identifier.</summary>
        /// <value>The identifier.</value>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>Gets or sets the name of the user.</summary>
        /// <value>The name of the user.</value>
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        /// <summary>Gets or sets the first name.</summary>
        /// <value>The first name.</value>
        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        /// <summary>Gets or sets the last name.</summary>
        /// <value>The last name.</value>
        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        /// <summary>Gets or sets the email.</summary>
        /// <value>The email.</value>
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonIgnore]
        public string UserPrincipleName { get; set; }

    }
}
