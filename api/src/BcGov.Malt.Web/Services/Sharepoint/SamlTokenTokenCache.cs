using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BcGov.Malt.Web.Services.Sharepoint
{
    public class SamlTokenTokenCache : TokenCache<SamlTokenParameters, string>
    {
        public SamlTokenTokenCache(IMemoryCache memoryCache, ILogger<SamlTokenTokenCache> logger) 
            : base(memoryCache, logger)
        {
        }

        protected override string GetCacheKey(SamlTokenParameters configuration)
        {
            StringBuilder buffer = new StringBuilder();

            // create a cache key based on all the parameters
            buffer.Append(configuration.RelyingParty);
            buffer.Append('|');
            buffer.Append(configuration.Username);
            buffer.Append('|');
            buffer.Append(configuration.StsUrl);
            buffer.Append('|');

            return buffer.ToString();
        }
    }
}
