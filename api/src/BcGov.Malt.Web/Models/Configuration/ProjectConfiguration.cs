using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace BcGov.Malt.Web.Models.Configuration
{
    /// <summary>
    /// Contains the configuration information regarding projects.
    /// The <see cref="Project"/> has overlap with this type but does not include 
    /// credentials for the resources.
    /// </summary>
    public class ProjectConfiguration
    {
        private string _name;

        /// <summary>
        /// The id of the project
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name 
        { 
            get => _name; 
            set
            {
                Id = GetHash(value ?? string.Empty);
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the resources in this project.
        /// </summary>
        /// <value>
        /// The resources.
        /// </value>
        public List<ProjectResource> Resources { get; set; }

        private static string GetHash(string value)
        {
#pragma warning disable CA5350 // a weak cryptographic algorithm SHA1
            // SHA1 should be fine as we are not using this value as a password hash
            using HashAlgorithm hashAlgorithm = SHA1.Create();
#pragma warning restore 

            var byteArray = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value));

            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
            }

            return hex.ToString();

        }
    }
}
