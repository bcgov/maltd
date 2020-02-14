using BcGov.Malt.Web.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Reflection;


namespace BcGov.Malt.Web
{
    /// <summary>
    /// The Startup class configures services and the app's request pipeline.
    /// </summary>
    public class Startup
    {
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
                options.AddDefaultPolicy(builder => {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                });
            });

            services.AddMvcCore()
                .AddJsonOptions(options => { options.JsonSerializerOptions.IgnoreNullValues = true; })
                .AddApiExplorer();

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
            
            // singleton for now since these are in memory (testing) implementations
            services.AddSingleton<IProjectService, InMemoryProjectService>();
            services.AddSingleton<IUserSearchService, InMemoryUserSearchService>();
            services.AddSingleton<IUserManagementService, InMemoryUserManagementService>();
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
