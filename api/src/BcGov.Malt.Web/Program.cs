using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// this should go in AssemblyInfo.cs
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BcGov.Malt.Web.Tests")]

namespace BcGov.Malt.Web
{
    /// <summary>
    /// The Program class represents the main entry point to the API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point.
        /// </summary>
        public static async Task<int> Main(string[] args)
        {
            ILogger logger = ConfigureLogging();

            try
            {
                // create the host
                IHost host = CreateHostBuilder(args).Build();

                // try to load all the access tokens on startup
                await GetAccessTokensAsync(logger, host.Services);

                logger.Information("Starting web host");
                await host.RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Web host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Added before AddUserSecrets to let user secrets override environment variables.
                    config.AddEnvironmentVariables();

                    var env = hostingContext.HostingEnvironment;
                    if (env.IsDevelopment())
                    {
                        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                        config.AddUserSecrets(appAssembly, optional: true);
                    }
                });

                return builder;
        }

        private static ILogger ConfigureLogging()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("appsettings.json");

            // Added before AddUserSecrets to let user secrets override environment variables.
            configurationBuilder.AddEnvironmentVariables();

#if DEBUG
            // use appsettings.Development.json and user secrets on debug builds
            configurationBuilder.AddJsonFile("appsettings.Development.json", optional: true);
            configurationBuilder.AddUserSecrets(typeof(Program).Assembly, optional: true);
#endif

            var configuration = configurationBuilder.Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Logger = logger;

            return logger;
        }


        private static async Task GetAccessTokensAsync(ILogger logger, IServiceProvider services)
        {

            try
            {
                logger.Information("Getting access tokens for all resources");

                using (var scope = services.CreateScope())
                {
                    // get the token loader/initializer
                    var accessTokenLoader = scope.ServiceProvider.GetRequiredService<AccessTokenLoader>();

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    List<Tuple<ProjectResource, Exception>> results = await accessTokenLoader.GetAccessTokensAsync();
                    stopwatch.Stop();
                    var milliseconds = stopwatch.Elapsed.TotalMilliseconds;

                    var exceptionCount = results.Count(_ => _.Item2 != null);
                    if (exceptionCount == 0)
                    {
                        logger.Information("Fetched all {AccessTokenCount} access tokens successfully, process took {Elapsed} milliseconds", results.Count, milliseconds);
                    }
                    else
                    {
                        if (exceptionCount == results.Count)
                        {
                            logger.Error("Error fetching all {AccessTokenCount} access tokens, process took {Elapsed} milliseconds", results.Count, milliseconds);
                        }
                        else
                        {
                            logger.Error("Error fetching {ErrorCount} of total {AccessTokenCount} access tokens, process took {Elapsed} milliseconds",
                                exceptionCount,
                                results.Count, 
                                milliseconds);

                        }

                        // log out which resources failed
                        foreach (var failed in results.Where(_ => _.Item2 != null))
                        {
                            var exception = failed.Item2;
                            var projectResource = failed.Item1;

                            if (projectResource.Type == ProjectType.SharePoint)
                            {
                                logger.Warning(exception, "Failed to load access token for {@ProjectResource}", new
                                {
                                    // log enough information to help identify the issue without actually logging out any credentials/sensitive info
                                    projectResource.Type,
                                    projectResource.Resource,
                                    // only return the 6 first character of the client id
                                    ClientId = projectResource.ClientId.Substring(0, 6) + "...",
                                    projectResource.RelyingPartyIdentifier
                                });

                            }
                            else if (projectResource.Type == ProjectType.Dynamics)
                            {
                                logger.Warning(exception, "Failed to load access token for {@ProjectResource}", new
                                {
                                    // log enough information to help identify the issue without actually logging out any credentials/sensitive info
                                    projectResource.Type,
                                    projectResource.Resource,
                                    // only return the 6 first character of the client id
                                    ClientId = projectResource.ClientId.Substring(0, 6) + "..."
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Warning(exception, "Error getting access tokens for all resources");
            }
        }
    }
}
