using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Resources;
using System.Reflection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.Http;

namespace BcGov.Jag.AccountManagement.Server.Infrastructure;

public static class Extensions
{
    public static void AddTelemetry(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<AspNetCoreInstrumentationOptions>(options =>
        {
            options.Filter = AspNetCoreFilter;
        });

        builder.Services.Configure<HttpClientInstrumentationOptions>(options =>
        {
            options.Filter = HttpClientRequestFilter;
        });

        var resourceBuilder = GetResourceBuilder(builder);

        builder.Services.AddOpenTelemetryTracing(options =>
        {
            options
                .SetResourceBuilder(resourceBuilder)
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddSource(DiagnosticTrace.Source);

            var tracingExporter = GetTracingExporter(builder);

            switch (tracingExporter)
            {
                case "jaeger":
                    options.AddJaegerExporter();

                    builder.Services.Configure<JaegerExporterOptions>(builder.Configuration.GetSection("Jaeger"));

                    // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
                    //builder.Services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
                    break;

                case "zipkin":
                    options.AddZipkinExporter();

                    builder.Services.Configure<ZipkinExporterOptions>(builder.Configuration.GetSection("Zipkin"));
                    break;
            }

        });

    }

    private static ResourceBuilder GetResourceBuilder(WebApplicationBuilder builder)
    {
        var serviceName = "Jag.AccountManagement";
        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

        var tracingExporter = GetTracingExporter(builder);

        var resourceBuilder = tracingExporter switch
        {
            "jaeger" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Jaeger:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
            "zipkin" => ResourceBuilder.CreateDefault().AddService(builder.Configuration.GetValue<string>("Zipkin:ServiceName"), serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
            _ => ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: assemblyVersion, serviceInstanceId: Environment.MachineName),
        };

        return resourceBuilder;
    }

    private static string GetTracingExporter(WebApplicationBuilder builder)
    {
        // Switch between Zipkin/Jaeger by setting UseExporter in appsettings.json.
        return builder.Configuration.GetValue<string>("UseTracingExporter").ToLowerInvariant();
    }

    private static bool HttpClientRequestFilter(HttpRequestMessage httpContext)
    {
        //if (IsSeqLoggingRequest(httpContext)) return false;
        return true;
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

    //private static bool IsSeqLoggingRequest(HttpRequestMessage request)
    //{
        
    //    return request.Method == HttpMethod.Post && request.RequestUri is not null && request.R
    //}
}
