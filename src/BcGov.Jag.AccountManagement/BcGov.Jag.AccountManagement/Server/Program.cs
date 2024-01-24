using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using BcGov.Jag.AccountManagement.Server.Infrastructure;
using BcGov.Jag.AccountManagement.Server.Services;
using BcGov.Jag.AccountManagement.Server.Services.Sharepoint;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Models.Authorization;
using System.Reflection;
using Blazored.Toast;
using System.Security.Claims;
using BcGov.Jag.AccountManagement.Shared;
using Microsoft.IdentityModel.Logging;

var builder = WebApplication.CreateBuilder(args);
var logger = GetLogger(builder);

// Add services to the container.

builder.Host.UseSerilog((hostingContext, loggerConfiguration) => {
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers());
});

builder.AddTelemetry(logger);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//uncomment if you need to troubleshoot Identity Model exceptions
//Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string? audience = builder.Configuration["Jwt:Audience"];

        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = audience;

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = (context) =>
            {
                if (context.Principal is null)
                {
                    context.Fail("No authenticated");
                    return Task.CompletedTask;
                }

                string? accessToken = context.Request.Headers.Authorization.FirstOrDefault();

                if (audience is not null && accessToken is not null)
                {
                    // On the existing in coming ClaimsIdentity Claims collection, one of the claims will be
                    // resource_access. We could parse that, however, if we parse the access token, the front end
                    // and the server side will use the same code for getting the resource level roles

                    // strip Bearer from the auth header
                    var scheme = "Bearer ";
                    if (accessToken.StartsWith(scheme))
                    {
                        accessToken = accessToken[scheme.Length..];
                    }

                    IEnumerable<Claim> roles = ResourceRoleAccessor.GetResourceRolesFromAccessToken(audience, accessToken);
                    ClaimsIdentity identity = new(context.Principal.Identity, roles);
                    context.Principal = new(identity);
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddLdapUserSearch(builder.Configuration);
builder.Services.AddMemoryCache();

builder.Services.ConfigureProjectResources(builder.Configuration, logger);

builder.Services.AddTransient<IUserManagementService, UserManagementService>();
builder.Services.AddTransient<IODataClientFactory, DefaultODataClientFactory>();

// Add HttpClient and IHttpClientFactory in case the project resources do not register it
// The DefaultODataClientFactory has dependency on IHttpClientFactory.
builder.Services.AddHttpClient();

// SAML services
// token caches are singleton because they maintain a per instance prefix
// that can be changed to effectively clear the cache
builder.Services.AddSingleton<ITokenCache<SamlTokenParameters, string>, SamlTokenTokenCache>();
builder.Services.AddTransient<ISamlAuthenticator, SamlAuthenticator>();

// OAuth services
// token caches are singleton because they maintain a per instance prefix
// that can be changed to effectively clear the cache
builder.Services.AddSingleton<ITokenCache<OAuthOptions, Token>, OAuthTokenCache>();
builder.Services.AddTransient<IOAuthClientFactory, OAuthClientFactory>();

builder.Services.AddTransient<IAccessTokenLoader, AccessTokenLoader>();
builder.Services.AddBlazoredToast();

var app = builder.Build();

// adjust the path base if set. Used when the application is run from a sub-app / sub-path
// see https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/?view=aspnetcore-7.0&tabs=visual-studio#app-base-path
var pathBase = app.Configuration.GetValue<string>("PathBase");
if (!string.IsNullOrEmpty(pathBase))
{
    logger.Information("Using path base {PathBase}");
    app.UsePathBase(pathBase);
}
else
{
    logger.Information("No path base set");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();


/// <summary>
/// Gets a logger for application setup.
/// </summary>
/// <returns></returns>
static Serilog.ILogger GetLogger(WebApplicationBuilder builder)
{
    var configuration = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.Console();

    if (builder.Environment.IsDevelopment())
    {
        configuration.WriteTo.Debug();
    }

    return configuration.CreateLogger();
}
