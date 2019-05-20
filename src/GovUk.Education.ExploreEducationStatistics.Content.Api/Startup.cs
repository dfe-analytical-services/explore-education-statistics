using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                    options.SerializerSettings.Converters.Add(new ContentBlockConverter());
                });

            var connectionString = Configuration.GetConnectionString("ContentDb");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                const string connection = "Data Source=dfe-meta.db";
                
                services.AddDbContext<ApplicationDbContext>(options =>
                    options
                        .UseSqlite(connection,
                            builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                        .EnableSensitiveDataLogging()
                );
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options
                        .UseSqlServer(connectionString,
                            builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName)));
            }

            // Adds Brotli and Gzip compressing
            services.AddResponseCompression();
            
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(swag =>
            {
                swag.SwaggerDoc("v1",
                    new Info {Title = "Explore education statistics - Content API", Version = "v1"});
            });

            services.AddCors();
            services.AddAutoMapper();
            services.AddTransient<IContentService, ContentService>();
            services.AddTransient<IMethodologyService, MethodologyService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            UpdateDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Content API V1");
                c.RoutePrefix = "docs";
            });

            app.UseHttpsRedirection();
            app.UseCors(options => options.WithOrigins("http://localhost:3000", "http://localhost:3001").AllowAnyMethod());
            app.UseMvc();

            var option = new RewriteOptions();
            option.AddRedirect("^$", "docs");
            app.UseRewriter(option);
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}