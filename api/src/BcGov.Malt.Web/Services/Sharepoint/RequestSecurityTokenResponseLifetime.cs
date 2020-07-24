using System;

namespace BcGov.Malt.Web.Services.Sharepoint
{
    public class RequestSecurityTokenResponseLifetime
    {
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Expires { get; set; }
    }
}