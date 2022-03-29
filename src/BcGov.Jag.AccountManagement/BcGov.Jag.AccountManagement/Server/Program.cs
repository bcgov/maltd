using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using BcGov.Jag.AccountManagement.Server.Services;
using BcGov.Jag.AccountManagement.Server.Services.Sharepoint;
using BcGov.Jag.AccountManagement.Server.Models.Configuration;
using BcGov.Jag.AccountManagement.Server.Models.Authorization;
using MediatR;
using System.Reflection;
using Blazored.Toast;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Host.UseSerilog((hostingContext, loggerConfiguration) => {
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers());
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
    });

builder.Services.AddMediatR(Assembly.GetExecutingAssembly());

builder.Services.AddLdapUserSearch(builder.Configuration);
builder.Services.AddMemoryCache();

builder.Services.ConfigureProjectResources(builder.Configuration, Serilog.Log.Logger); // TODO: pass in a configured logger

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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
