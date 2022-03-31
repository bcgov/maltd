using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Shared;

namespace BcGov.Jag.AccountManagement.Server.Services;

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
