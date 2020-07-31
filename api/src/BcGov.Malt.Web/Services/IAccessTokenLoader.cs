using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;

namespace BcGov.Malt.Web.Services
{
    /// <summary>
    /// Represents a service that will attempt to get access tokens for each configured project resource.
    /// </summary>
    public interface IAccessTokenLoader
    {
        /// <summary>
        /// Gets the access tokens asynchronously.
        /// </summary>
        /// <returns>
        /// A list of project resources and any exceptions thrown while getting the access token.
        /// </returns>
        Task<List<Tuple<ProjectResource, Exception>>> GetAccessTokensAsync();
    }
}