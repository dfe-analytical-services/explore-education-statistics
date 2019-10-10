using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Converters;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _logger = logger;
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

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                    .EnableSensitiveDataLogging()
            );

            // Adds Brotli and Gzip compressing
            services.AddResponseCompression();
            
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(swag =>
            {
                swag.SwaggerDoc("v1",
                    new Info {Title = "Explore education statistics - Content API", Version = "v1"});
            });

            services.AddCors();
            
            var cloudStorageAccount = CloudStorageAccount.Parse(Configuration.GetConnectionString("PublicStorage"));
            services.AddSingleton<CloudBlobClient>(a => cloudStorageAccount.CreateCloudBlobClient());
            
            services.AddTransient<IContentCacheService, ContentCacheService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            UpdateDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                GenerateContentCache();
            }
            else
            {
                app.UseHttpsRedirection();
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

            app.UseCors(options => options.WithOrigins("http://localhost:3000", "http://localhost:3001","https://localhost:3000","https://localhost:3001").AllowAnyMethod().AllowAnyHeader());
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

        // TODO: this should only be used in development, adds the required message to the content-cache queue
        private void GenerateContentCache()
        {
            CloudQueueClient cloudStorageClient = null;
            
            try
            {
                var cloudStorageAccount = CloudStorageAccount.Parse(Configuration.GetConnectionString("PublicStorage"));
                cloudStorageClient = cloudStorageAccount.CreateCloudQueueClient();
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to create content-cache queue client");
                throw;
            }

            try
            {
                var queue = cloudStorageClient.GetQueueReference("content-cache");
                queue.CreateIfNotExists();
                
                var message = new CloudQueueMessage("Generation triggered by the Content API Startup");
                queue.AddMessage(message);
                
                _logger.LogInformation("Message added to content-cache queue.");
                _logger.LogInformation("Please ensure the publisher function is running");
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable add message to content-cache queue");
                throw;
            }
        }
    }
}