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

namespace BcGov.Malt.Web
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
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
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();


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


            services.AddTransient<IUserSearchService, UserSearchService>();

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }

            app.UseHttpsRedirection();

            app.UseRouting();

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
