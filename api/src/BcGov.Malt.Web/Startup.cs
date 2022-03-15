using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using BcGov.Malt.Web.HealthChecks;
using BcGov.Malt.Web.Models.Authorization;
using BcGov.Malt.Web.Models.Configuration;
using BcGov.Malt.Web.Services;
using BcGov.Malt.Web.Services.Sharepoint;
using HealthChecks.UI.Client;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BcGov.Malt.Web
{
    /// <summary>
    /// The Startup class configures services and the app's request pipeline.
    /// </summary>
    public class Startup
    {
        private static readonly Serilog.ILogger Log = Serilog.Log.ForContext<Startup>();
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="configuration"></param>
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;

            _environment = env;
        }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The service description collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // all endpoint must be authenticated
            services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);

            // send header Access-Control-Allow-Origin: *
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                });
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(ConfigureJwtBearerAuthentication);

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // add all the handlers in this assembly
            services.AddMediatR(GetType().Assembly);

            AddSwaggerGen(services);

            services.AddMemoryCache();

#if false
            AddHealthChecks(services);

            services
                .AddHealthChecksUI()
                .AddInMemoryStorage();
#endif

            // this will configure the service correctly, comment out for now until
            // the services are working
            services.ConfigureProjectResources(Configuration, Log);

            services.AddTransient<IUserSearchService, LdapUserSearchService>();
            services.AddTransient<IUserManagementService, UserManagementService>();
            services.AddTransient<IODataClientFactory, DefaultODataClientFactory>();


            // Add HttpClient and IHttpClientFactory in case the project resources do not register it
            // The DefaultODataClientFactory has dependency on IHttpClientFactory.
            services.AddHttpClient();

            // SAML services
            // token caches are singleton because they maintain a per instance prefix
            // that can be changed to effectively clear the cache
            services.AddSingleton<ITokenCache<SamlTokenParameters, string>, SamlTokenTokenCache>();
            services.AddTransient<ISamlAuthenticator, SamlAuthenticator>();

            // OAuth services
            // token caches are singleton because they maintain a per instance prefix
            // that can be changed to effectively clear the cache
            services.AddSingleton<ITokenCache<OAuthOptions, Token>, OAuthTokenCache>();
            services.AddTransient<IOAuthClientFactory, OAuthClientFactory>();

            services.AddTransient<IAccessTokenLoader, AccessTokenLoader>();

            void ConfigureJwtBearerAuthentication(JwtBearerOptions o)
            {
                /* TODO Add keycloak authentication params once its ready .The following may change in future
                 * For now , Jwt:Authority = the url to your local keycloak relam , eg: Master or ISB
                 *               Audience = the name of the client you create in keycloak relam ,e.g: maltd or demo-app 
                 *   "Jwt":{
                             "Authority": "http://localhost:8080/auth/realms/<realm-name>",
                             "Audience": "maltd"
                            }
                 */

                string audience = Configuration["Jwt:Audience"];
                string authority = Configuration["Jwt:Authority"];

                // Jwt:Audience and Jwt:Authority are required to validate authentication tokens.
                if (string.IsNullOrEmpty(audience))
                {
                    Log.Fatal("Required configuration item {Setting} is not set", "Jwt:Audience");
                }

                if (string.IsNullOrEmpty(authority))
                {
                    Log.Fatal("Required configuration item {Setting} is not set", "Jwt:Authority");
                }

                if (string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(authority))
                {
                    throw new ConfigurationErrorsException("One or more required configuration parameters are missing Jwt:Audience or Jwt:Authority");
                }

                if (!authority.EndsWith("/", StringComparison.InvariantCulture))
                {
                    authority += "/";
                }

                // KeyCloak metadata address https://www.keycloak.org/docs/4.8/authorization_services/#_service_authorization_api
                string metadataAddress = authority + ".well-known/uma2-configuration";

                o.Authority = authority;
                o.Audience = audience;
                o.MetadataAddress = metadataAddress;

                o.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();

                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "text/plain";

                        if (_environment.IsDevelopment())
                        {
                            return c.Response.WriteAsync(c.Exception.ToString());
                        }

                        return c.Response.WriteAsync("An error occured processing your authentication.");
                    }
                };
            }
        }

        private void AddHealthChecks(IServiceCollection services)
        {
            var healthCheckBuilder = services.AddHealthChecks();

            foreach (var project in Configuration.GetProjectConfigurations(Log))
            {
                string healthCheckName = project.Name + " Access Token Health Check";

                IHealthCheck HealthCheckFactory(IServiceProvider serviceProvider)
                {
                    var factory = serviceProvider.GetRequiredService<IProjectAccessTokenHealthCheckFactory>();
                    return factory.Create(project);
                }

                healthCheckBuilder.Add(new HealthCheckRegistration(healthCheckName, HealthCheckFactory, null,
                    new[] { "access_token" }));
            }

            // add the required services for the the health checks
            services.AddTransient<IProjectAccessTokenHealthCheckFactory, ProjectAccessTokenHealthCheckFactory>();
        }


        private void AddSwaggerGen(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                // name here shows up in the OpenAPI title
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Dynamics Account Management API",
                    Version = "v1",
                    Contact = new OpenApiContact
                    {
                        Email = "apiteam@example.org",
                        Name = "API Team",
                        Url = new Uri("https://github.com/bcgov/maltd")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Apache 2.0",
                        Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0.html")
                    }
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = @"JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

                options.OperationFilter<ConsumesAndProductOnlyJSonContentTypeFilter>();
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // Apply CORS policies to all endpoints
            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHealthChecksUI();

            app.UseEndpoints(endpoints =>
            {
                // disable the authentication if debugging locally 
                var endpointConventionBuilder = endpoints.MapControllers();
                if (!env.IsDevelopment())
                {
                    // make it easier for testing to only require authorization in non development
                    endpointConventionBuilder.RequireAuthorization();
                }

#if false
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                    {
                        Predicate = registration => true,
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    })
                    /*.RequireAuthorization()*/
                    ;

                endpoints.MapHealthChecksUI();
#endif
            });


                if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    // name here shows up in the 'Select a definition' drop down
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dynamics Account Management API V1");
                });
            }
        }

        internal class ConsumesAndProductOnlyJSonContentTypeFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                // feels like there should be a better way than doing this on each operation
                RemoveNonJsonResponseTypes(operation);
            }

            private void RemoveNonJsonResponseTypes(OpenApiOperation operation)
            {
                foreach (var response in operation.Responses)
                {
                    var content = response.Value.Content;

                    var keys = content.Keys;
                    foreach (var key in keys)
                    {
                        if (key != "application/json")
                        {
                            content.Remove(key);
                        }
                    }
                }
            }
        }
    }
}
