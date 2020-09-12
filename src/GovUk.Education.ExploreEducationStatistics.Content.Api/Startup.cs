using System.Diagnostics.CodeAnalysis;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

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
            services.AddApplicationInsightsTelemetry();
            services.AddMvc(options => { options.EnableEndpointRouting = false; })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            // Adds Brotli and Gzip compressing
            services.AddResponseCompression();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(swag =>
            {
                swag.SwaggerDoc("v1",
                    new OpenApiInfo {Title = "Explore education statistics - Content API", Version = "v1"});
            });

            services.AddCors();
            services.AddTransient<IBlobStorageService, BlobStorageService>(
                s => new BlobStorageService(
                    Configuration.GetValue<string>("PublicStorage"),
                    s.GetRequiredService<ILogger<BlobStorageService>>()
                )
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                PublishAllContent(logger);
            }
            else
            {
                app.UseHttpsRedirection();
                app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
            }

            if(env.IsDevelopment() || Configuration.GetValue<bool>("enableSwagger"))
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Content API V1");
                    c.RoutePrefix = "docs";
                });

                var option = new RewriteOptions();
                option.AddRedirect("^$", "docs");
                app.UseRewriter(option);
            }

            app.UseCors(options => options.WithOrigins("http://localhost:3000", "http://localhost:3001","https://localhost:3000","https://localhost:3001").AllowAnyMethod().AllowAnyHeader());
            app.UseMvc();
        }

        /**
         * Add a message to the queue to publish all content.
         * This should only be used in development!
         */
        private void PublishAllContent(ILogger logger)
        {
            const string queueName = PublishAllContentQueue;
            try
            {
                var storageQueueService = new StorageQueueService(Configuration.GetConnectionString("PublisherStorage"));
                storageQueueService.AddMessages(queueName, new PublishAllContentMessage());

                logger.LogInformation($"Message added to {queueName} queue");
                logger.LogInformation("Please ensure the Publisher function is running");
            }
            catch
            {
                logger.LogError($"Unable add message to {queueName} queue");
                throw;
            }
        }
    }
}