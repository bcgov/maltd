using System;

namespace BcGov.Malt.Web.Infrastructure
{
    public static class ApiGatewayUriBuilder
    {
        public static Uri Build(Uri source, string host, string policy)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(policy))
            {
                return source;
            }

            // https://<host>/policy
            UriBuilder builder = new UriBuilder(source);
            builder.Host = host;

            // inject the policy name as the first uri segment
            builder.Path = builder.Path.StartsWith("/", StringComparison.InvariantCulture)
                ? "/" + policy + builder.Path
                : "/" + policy + "/" + builder.Path;

            return builder.Uri;
        }
    }
}
