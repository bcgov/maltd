using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Gets or sets the resources.
        /// </summary>
        /// <value>
        /// The resources.
        /// </value>
        [JsonPropertyName("resources")]
        public List<ProjectResourceStatus> Resources { get; set; }
    }

    public class ProjectResourceStatus
    {
        ////public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the resource
        /// </summary>
        /// <value>
        /// The type will be Dynamics or Sharepoint
        /// </value>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status can be: member, not-member, error
        /// </value>
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
