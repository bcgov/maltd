using System;
using System.Net;
using Serilog;

namespace BcGov.Malt.Web.Infrastructure
{
    public static class ServicePointManagerConfiguration
    {
        private const string EnvironmentVariable = "DEFAULTCONNECTIONLIMIT";

        public static void Configure(ILogger logger)
        {
            // DefaultConnectionLimit : The maximum number of concurrent connections allowed to a single host (ServicePoint)
            int defaultConnectionLimit;


            var value = Environment.GetEnvironmentVariable(EnvironmentVariable);
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out defaultConnectionLimit))
            {
                logger.Information("Environment variable {EnvironmentVariable} is set to {DefaultConnectionLimit}", EnvironmentVariable, defaultConnectionLimit);
            }
            else
            {
                logger.Information("Environment variable {EnvironmentVariable} is not set or is not an integer, defaulting DefaultConnectionLimit to 50", EnvironmentVariable);
                defaultConnectionLimit = 50;
            }

            ServicePointManager.DefaultConnectionLimit = defaultConnectionLimit;

            logger.Information("ServicePointManager settings = {@ServicePointManager}",
                new
                {
                    ServicePointManager.UseNagleAlgorithm,
                    ServicePointManager.DnsRefreshTimeout,
                    ServicePointManager.Expect100Continue,
                    ServicePointManager.CheckCertificateRevocationList,
                    ServicePointManager.MaxServicePoints,
                    ServicePointManager.MaxServicePointIdleTime,
                    ServicePointManager.DefaultConnectionLimit,
                    ServicePointManager.DefaultPersistentConnectionLimit
                });
        }
    }
}
