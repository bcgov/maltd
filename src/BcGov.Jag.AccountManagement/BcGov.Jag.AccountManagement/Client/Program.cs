using BcGov.Jag.AccountManagement.Client;
using BcGov.Jag.AccountManagement.Client.Authentication;
using BcGov.Jag.AccountManagement.Client.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.Toast;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.UseData();

builder.Services
    .AddOidcAuthentication(options => { builder.Configuration.Bind("oidc", options.ProviderOptions); })
    .AddAccountClaimsPrincipalFactory<KeycloakAccountClaimsPrincipalFactory<RemoteUserAccount>>()
    ;

builder.Services.AddApiAuthorization();
builder.Services.AddBlazoredToast();

await builder.Build().RunAsync();
