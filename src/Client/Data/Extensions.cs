using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;

namespace BcGov.Jag.AccountManagement.Client.Data;

public static class Extensions
{
    public static WebAssemblyHostBuilder UseData(this WebAssemblyHostBuilder builder)
    {
        var baseUri = new Uri(builder.HostEnvironment.BaseAddress);

        // check to see if the app is running in a sub-path
        var pathBase = builder.Configuration.GetValue<string>("PathBase");
        if (!string.IsNullOrEmpty(pathBase))
        {
            baseUri = new Uri(baseUri, pathBase);
        }

        var apiUri = new Uri(baseUri, "api");

        // only send the access token on requests to the API
        builder.Services
            .AddRefitClient<IUserApi>()
            .ConfigureHttpClient(client => {
                client.BaseAddress = baseUri;
                client.Timeout = TimeSpan.FromMinutes(5);
            })
            .AddHttpMessageHandler(services =>
                services.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(new[] { apiUri.ToString() }));

        // register the repository
        builder.Services.AddTransient<IRepository, Repository>();

        return builder;
    }
}
