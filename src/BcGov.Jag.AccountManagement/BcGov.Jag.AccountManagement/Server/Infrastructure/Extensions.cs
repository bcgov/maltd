using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Instrumentation.Http;

namespace BcGov.Jag.AccountManagement.Server.Infrastructure;

public static class Extensions
{
    public static void AddTelemetry(this WebApplicationBuilder builder, Serilog.ILogger logger)
    {
        string? endpoint = builder.Configuration["OTEL_EXPORTER_JAEGER_ENDPOINT"];

        if (string.IsNullOrEmpty(endpoint))
        {
            logger.Information("Jaeger endpoint is not configured, no telemetry will be collected.");
            return;
        }

        builder.Services.Configure<AspNetCoreInstrumentationOptions>(options =>
        {
            options.Filter = AspNetCoreFilter;
        });

        builder.Services.Configure<HttpClientInstrumentationOptions>(options =>
        {
            options.Filter = HttpClientRequestFilter;
        });

        var resourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(Diagnostics.Source.Name, serviceInstanceId: Environment.MachineName);

        builder.Services.AddOpenTelemetry()
            .WithTracing(configure =>
            {
                configure
                    .SetResourceBuilder(resourceBuilder)
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.Filter = HttpClientRequestFilter;
                    })
                    .AddAspNetCoreInstrumentation()
                    .AddSource(Diagnostics.Source.Name)
                    .AddJaegerExporter();
            });
    }

    private static bool HttpClientRequestFilter(HttpRequestMessage message)
    {
        var path = message.RequestUri?.LocalPath ?? string.Empty;

        // do not trace calls to splunk or seq
        bool isNotLoggingPath = path != "/services/collector" && path != "/api/events/raw";
        return isNotLoggingPath;
    }

    private static bool AspNetCoreFilter(HttpContext httpContext)
    {
        var request = httpContext.Request;
        if (request.Method != "GET") return true; // instrument this request

        var path = request.Path.ToUriComponent(); // get the path as string

        if (path.StartsWith("/_framework/")) return false;
        if (path.StartsWith("/authentication")) return false;

        if (path == "/_vs/browserLink") return false;

        var response = httpContext.Response;
        if (response.StatusCode == 404 || response.StatusCode == 0)
        {
            if (path.Contains("/_framework/")) return false;
        }

        return true;
    }
}
