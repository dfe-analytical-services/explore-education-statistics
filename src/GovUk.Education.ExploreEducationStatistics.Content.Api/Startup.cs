using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;
using IThemeService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IThemeService;
using ThemeService = GovUk.Education.ExploreEducationStatistics.Content.Services.ThemeService;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private IHostEnvironment HostEnvironment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
            
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddApplicationInsightsTelemetry()
                .AddApplicationInsightsTelemetryProcessor<SensitiveDataTelemetryProcessor>();

            services.AddMvc(options =>
                {
                    options.Filters.Add(new OperationCancelledExceptionFilter());
                    options.EnableEndpointRouting = false;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddControllers(
                options =>
                {
                    options.ModelBinderProviders.Insert(0, new SeparatedQueryModelBinderProvider(","));
                }
            );

            services.AddDbContext<StatisticsDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("StatisticsDb"),
                        builder => builder.MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model"))
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            services.AddDbContext<ContentDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        builder => builder.MigrationsAssembly(typeof(Startup).Assembly.FullName))
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            // Adds Brotli and Gzip compressing
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(swag =>
            {
                swag.SwaggerDoc("v1",
                    new OpenApiInfo {Title = "Explore education statistics - Content API", Version = "v1"});
            });

            services.AddCors();
            services.AddSingleton<IBlobStorageService, BlobStorageService>(provider =>
                {
                    var connectionString = Configuration.GetValue<string>("PublicStorage");

                    return new BlobStorageService(
                        connectionString,
                        new BlobServiceClient(connectionString),
                        provider.GetRequiredService<ILogger<BlobStorageService>>(),
                        new StorageInstanceCreationUtil()
                    );
                }
            );
            services.AddTransient<IBlobCacheService, BlobCacheService>();
            services.AddTransient<IFileStorageService, FileStorageService>();
            services.AddTransient<IFilterRepository, FilterRepository>();
            services.AddTransient<IIndicatorRepository, IndicatorRepository>();
            services.AddTransient<IDataGuidanceService, DataGuidanceService>();
            services.AddTransient<Services.Interfaces.IPublicationService, Services.PublicationService>();
            services.AddTransient<ITimePeriodService, TimePeriodService>();
            services.AddTransient<IDataGuidanceSubjectService, DataGuidanceSubjectService>();
            services.AddTransient<IFootnoteRepository, FootnoteRepository>();
            services.AddTransient<IMethodologyImageService, MethodologyImageService>();
            services.AddTransient<IMethodologyService, MethodologyService>();
            services.AddTransient<IMethodologyRepository, MethodologyRepository>();
            services.AddTransient<IMethodologyVersionRepository, MethodologyVersionRepository>();
            services.AddTransient<IThemeService, ThemeService>();
            services.AddTransient<IReleaseService, Services.ReleaseService>();
            services.AddTransient<IReleaseFileService, ReleaseFileService>();
            services.AddTransient<IReleaseDataFileRepository, ReleaseDataFileRepository>();
            services.AddTransient<IDataGuidanceFileWriter, DataGuidanceFileWriter>();
            services.AddTransient<IGlossaryService, GlossaryService>();

            StartupSecurityConfiguration.ConfigureAuthorizationPolicies(services);
            StartupSecurityConfiguration.ConfigureResourceBasedAuthorization(services);

            AddPersistenceHelper<ContentDbContext>(services);
            AddPersistenceHelper<StatisticsDbContext>(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IWebHostEnvironment env,
            ILogger<Startup> logger)
        {
            // Enable caching and register any caching services
            CacheAspect.Enabled = true;
            BlobCacheAttribute.AddService("default", app.ApplicationServices.GetService<IBlobCacheService>());

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

            if(Configuration.GetValue<bool>("enableSwagger"))
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

            app.UseCors(options => options
                .WithOrigins(
                    "http://localhost:3000",
                    "http://localhost:3001",
                    "https://localhost:3000",
                    "https://localhost:3001")
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseMvc();
            app.UseHealthChecks("/api/health");

            app.UseResponseCompression();
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
                var publisherConnectionString = Configuration.GetValue<string>("PublisherStorage");
                var storageQueueService = new StorageQueueService(
                    publisherConnectionString,
                    new StorageInstanceCreationUtil());
                storageQueueService.AddMessage(queueName, new PublishAllContentMessage());

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
