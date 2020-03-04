using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace BcGov.Malt.Web.Models
{
    /// <summary>
    /// Represents a project that a user can be granted access to.
    /// </summary>
    public class Project
    {
        public Project()
        {
        }

        public Project(string id, string name)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Id cannot be null or empty", nameof(id));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            Id = id;
            Name = name;
        }

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
    }
}
