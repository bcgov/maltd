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

        builder.Services.AddOpenTelemetryTracing(options =>
        {
            options
                .SetResourceBuilder(resourceBuilder)
                .AddHttpClientInstrumentation(options =>
                {                    
                    options.Filter = HttpClientRequestFilter;
                })
                .AddAspNetCoreInstrumentation()
                .AddSource(Diagnostics.Source.Name)
                .AddJaegerExporter();


            // if we need to coustomize the exporter options
            ////builder.Services.Configure<JaegerExporterOptions>(...);
        });

    }

    private static bool HttpClientRequestFilter(HttpRequestMessage message)
    {
        // do not trace calls to splunk
        return message.RequestUri?.Host != "hec.monitoring.ag.gov.bc.ca";
    }

    private static bool AspNetCoreFilter(HttpContext httpContext)
    {
        if (IsWebAssemblyRequest(httpContext)) return false;
        return true;

    }

    private static bool IsWebAssemblyRequest(HttpContext httpContext)
    {
        return httpContext.Request.Method == "GET" && httpContext.Request.Path.StartsWithSegments("/_framework", StringComparison.OrdinalIgnoreCase);
    }
}
