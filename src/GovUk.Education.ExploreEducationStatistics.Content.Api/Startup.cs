using System;
using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.ModelBinding;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;

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
                .AddNewtonsoftJson(options =>
                {
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
                        providerOptions => 
                            providerOptions
                                .MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model")
                                .EnableCustomRetryOnFailure()
                            )
                    .EnableSensitiveDataLogging(HostEnvironment.IsDevelopment())
            );

            services.AddDbContext<ContentDbContext>(options =>
                options
                    .UseSqlServer(Configuration.GetConnectionString("ContentDb"),
                        providerOptions => 
                            providerOptions
                                .MigrationsAssembly(typeof(Startup).Assembly.FullName)
                                .EnableCustomRetryOnFailure()
                            )
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
            services.AddSingleton<IMemoryCacheService>(provider =>
            {
                var memoryCacheConfig = Configuration.GetSection("MemoryCache");
                var maxCacheSizeMb = memoryCacheConfig.GetValue<int>("MaxCacheSizeMb");
                var expirationScanFrequencySeconds = memoryCacheConfig.GetValue<int>("ExpirationScanFrequencySeconds");
                return new MemoryCacheService(
                    new MemoryCache(new MemoryCacheOptions
                    {
                        SizeLimit = maxCacheSizeMb * 1000000,
                        ExpirationScanFrequency = TimeSpan.FromSeconds(expirationScanFrequencySeconds),
                    }),
                    provider.GetRequiredService<ILogger<MemoryCacheService>>());
            });
            services.AddTransient<IFilterRepository, FilterRepository>();
            services.AddTransient<IIndicatorRepository, IndicatorRepository>();
            services.AddTransient<IDataGuidanceService, DataGuidanceService>();
            services.AddTransient<IPublicationCacheService, PublicationCacheService>();
            services.AddTransient<IPublicationRepository, PublicationRepository>();
            services.AddTransient<IPublicationService, PublicationService>();
            services.AddTransient<ITimePeriodService, TimePeriodService>();
            services.AddTransient<IDataGuidanceSubjectService, DataGuidanceSubjectService>();
            services.AddTransient<IFootnoteRepository, FootnoteRepository>();
            services.AddTransient<IMethodologyImageService, MethodologyImageService>();
            services.AddTransient<IMethodologyService, MethodologyService>();
            services.AddTransient<IMethodologyRepository, MethodologyRepository>();
            services.AddTransient<IMethodologyVersionRepository, MethodologyVersionRepository>();
            services.AddTransient<IMethodologyCacheService, MethodologyCacheService>();
            services.AddTransient<IReleaseCacheService, ReleaseCacheService>();
            services.AddTransient<IReleaseRepository, ReleaseRepository>();
            services.AddTransient<IReleaseService, Services.ReleaseService>();
            services.AddTransient<IReleaseFileRepository, ReleaseFileRepository>();
            services.AddTransient<IReleaseFileService, ReleaseFileService>();
            services.AddTransient<IReleaseFileBlobService, PublicReleaseFileBlobService>();
            services.AddTransient<IReleaseDataFileRepository, ReleaseDataFileRepository>();
            services.AddTransient<IDataGuidanceFileWriter, DataGuidanceFileWriter>();
            services.AddTransient<IGlossaryCacheService, GlossaryCacheService>();
            services.AddTransient<IGlossaryService, GlossaryService>();
            services.AddTransient<IThemeService, ThemeService>();

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
            BlobCacheAttribute.AddService("public", app.ApplicationServices.GetRequiredService<IBlobCacheService>());

            // Register the MemoryCacheService only if the Memory Caching is enabled. 
            var memoryCacheConfig = Configuration.GetSection("MemoryCache");
            if (memoryCacheConfig.GetValue("Enabled", false))
            {
                MemoryCacheAttribute.SetOverrideConfiguration(memoryCacheConfig.GetSection("Overrides"));
                MemoryCacheAttribute.AddService("default", app.ApplicationServices.GetService<IMemoryCacheService>()!);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
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
    }
}
