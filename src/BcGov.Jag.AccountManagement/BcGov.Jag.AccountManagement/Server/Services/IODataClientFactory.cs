using Simple.OData.Client;

namespace BcGov.Jag.AccountManagement.Server.Services;

/// <summary> 
///  
/// </summary> 
public interface IODataClientFactory
{
    /// <summary> 
    /// Creates the named <see cref="IODataClient"/>. 
    /// </summary> 
    /// <param name="name"></param> 
    /// <returns></returns> 
    IODataClient Create(string name);

    /// <summary>
    /// Creates a <see cref="HttpClient"/> for raw requests.
    /// Note, this is a work around so we can make raw requests to assign users to roles.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    HttpClient CreateHttpClient(string name);
}
