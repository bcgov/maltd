using System.Security.Cryptography;
using System.Text;

namespace BcGov.Malt.Web.Models.Configuration
{
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
        public ProjectResource[] Resources { get; set; }

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
