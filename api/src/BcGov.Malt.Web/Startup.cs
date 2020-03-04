using BcGov.Malt.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net.Http;
using System.Threading.Tasks;
using BcGov.Malt.Web.Models.Configuration;
using System.Threading;
using System.Net.Http.Headers;

namespace BcGov.Malt.Web
{
    /// <summary>
    /// The Startup class configures services and the app's request pipeline.
    /// </summary>
    public class Startup
    {
        private static readonly Serilog.ILogger _log = Serilog.Log.ForContext<Startup>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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
            services.AddControllers();

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

            services.AddMvcCore()
                .AddJsonOptions(options => { options.JsonSerializerOptions.IgnoreNullValues = true; })
                .AddApiExplorer();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                /* TODO Add keycloak authentication params once its ready .The following may change in future
                 * For now , Jwt:Authority = the url to your local keycloak relam , eg: Master or ISB
                 *               Audience = the name of the client you create in keycloak relam ,e.g: maltd or demo-app 
                 *   "Jwt":{
                             "Authority": "http://localhost:8080/auth/realms/<Relam_Name>",
                             "Audience": "maltd"
                } */
                o.Authority = Configuration["Jwt:Authority"];
                o.Audience = Configuration["Jwt:Audience"];
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();

                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "text/plain";

                        return c.Response.WriteAsync(c.Exception.ToString());
                                              
                    }
                };
            });

            AddSwaggerGen(services);

            services.AddMemoryCache();

            // this will configure the service correctly, comment out for now until
            // the services are working
            services.AddProjectAccess(Configuration);

            services.AddTransient<IProjectService, ProjectService>();
            services.AddTransient<IUserSearchService, LdapUserSearchService>();

            // singleton for now since these are in memory (testing) implementations
            //services.AddSingleton<IUserSearchService, InMemoryUserSearchService>();
            services.AddSingleton<IUserManagementService, InMemoryUserManagementService>();

            services.AddSingleton<IODataClientFactory, DefaultODataClientFactory>();

            services.AddTransient<ITokenCache, TokenCache>();
        }

        private void AddSwaggerGen(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                // name here shows up in the OpenAPI title
                c.SwaggerDoc("v1", new OpenApiInfo
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

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.OperationFilter<ConsumesAndProductOnlyJSonContentTypeFilter>();
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // name here shows up in the 'Select a definition' drop down
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dynamics Account Management API V1");
            });

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
