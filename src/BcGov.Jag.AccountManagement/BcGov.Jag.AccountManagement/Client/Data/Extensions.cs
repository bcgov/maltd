using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;

namespace BcGov.Jag.AccountManagement.Client.Data;

public static class Extensions
{
    public static WebAssemblyHostBuilder UseData(this WebAssemblyHostBuilder builder)
    {
        var baseAddress = builder.HostEnvironment.BaseAddress;

        // only send the access token on requests to the API
        builder.Services
            .AddRefitClient<IUserApi>()
            .ConfigureHttpClient(client => {
                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = TimeSpan.FromMinutes(5);
            })
            .AddHttpMessageHandler(services =>
                services.GetRequiredService<AuthorizationMessageHandler>()
                .ConfigureHandler(new[] { baseAddress + "api/" }));

        // register the repository
        builder.Services.AddTransient<IRepository, Repository>();

        return builder;
    }
}
