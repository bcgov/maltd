using System;

namespace BcGov.Malt.Web.Models.Configuration
{
    public class ProjectResource
    {
        /// <summary>
        /// The project type.
        /// </summary>
        public ProjectType Type { get; set; }

        /// <summary>
        /// The OAuth endpoint that issues the access tokens.
        /// </summary>
        public Uri AuthorizationUri { get; set; }

        /// <summary>
        /// The resource the access token is for.
        /// </summary>
        public Uri Resource { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
