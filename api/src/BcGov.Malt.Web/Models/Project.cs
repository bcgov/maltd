using System;
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
        public Project(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            Id = GetHash(name);
            Name = name;
        }

        /// <summary>
        /// The id of the project
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; }

        /// <summary>
        /// The name of the project
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        private static string GetHash(string value)
        {
            // SHA1 should be fine as we are not using this value as a password hash

            using HashAlgorithm hashAlgorithm = SHA1.Create();
            var byteArray = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));

            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();

        }
    }
}
