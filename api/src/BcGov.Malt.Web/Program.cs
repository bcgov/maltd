using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

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
        public static int Main(string[] args)
        {
            ConfigureLogging();

            try
            {
                Log.Logger.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Logger.Fatal(ex, "Web host terminated unexpectedly");
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

        private static void ConfigureLogging()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Logger = logger;
        }
    }
}
