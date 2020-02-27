using Simple.OData.Client;

namespace BcGov.Malt.Web.Services
{
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
    }
}
