#nullable enable
using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cancellation;
using GovUk.Education.ExploreEducationStatistics.Common.Config;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Rules;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.ReleaseService;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api;

[ExcludeFromCodeCoverage]
public class Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
{
    // This method gets called by the runtime. Use this method to add services to the container.
    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddHealthChecks();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services
            .AddApplicationInsightsTelemetry()
            .AddApplicationInsightsTelemetryProcessor<SensitiveDataTelemetryProcessor>();

        services
            .AddMvc(options =>
            {
                options.Filters.Add(new OperationCancelledExceptionFilter());
                options.Filters.Add(new ProblemDetailsResultFilter());
                options.EnableEndpointRouting = false;
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

        services.AddControllers(options =>
        {
            options.AddCommaSeparatedQueryModelBinderProvider();
            options.AddTrimStringBinderProvider();
        });

        services.AddFluentValidation();
        services.AddValidatorsFromAssembly(typeof(FullTableQueryRequest.Validator).Assembly); // Adds *all* validators from Common
        services.AddValidatorsFromAssembly(typeof(DataSetFileListRequest.Validator).Assembly); // Adds *all* validators from Content

        services.AddDbContext<StatisticsDbContext>(options =>
            options
                .UseSqlServer(
                    configuration.GetConnectionString("StatisticsDb"),
                    providerOptions =>
                        providerOptions
                            .MigrationsAssembly("GovUk.Education.ExploreEducationStatistics.Data.Model")
                            .EnableCustomRetryOnFailure()
                )
                .EnableSensitiveDataLogging(hostEnvironment.IsDevelopment())
        );

        services.AddDbContext<ContentDbContext>(options =>
            options
                .UseSqlServer(
                    configuration.GetConnectionString("ContentDb"),
                    providerOptions =>
                        providerOptions
                            .MigrationsAssembly(typeof(Startup).Assembly.FullName)
                            .EnableCustomRetryOnFailure()
                )
                .EnableSensitiveDataLogging(hostEnvironment.IsDevelopment())
        );

        // Adds Brotli and Gzip compressing
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
        });

        // Register the Swagger generator, defining 1 or more Swagger documents
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo { Title = "Explore education statistics - Content API", Version = "v1" }
            );
        });

        services.AddCors();

        // Options - to allow them to be injected into services
        services.AddOptions<AnalyticsOptions>().Bind(configuration.GetSection(AnalyticsOptions.Section));

        // Services
        services.AddSingleton<IBlobSasService, BlobSasService>();
        services.AddTransient<IPublicBlobStorageService, PublicBlobStorageService>(
            provider => new PublicBlobStorageService(
                connectionString: configuration.GetRequiredValue("PublicStorage"),
                logger: provider.GetRequiredService<ILogger<IBlobStorageService>>(),
                sasService: provider.GetRequiredService<IBlobSasService>()
            )
        );
        services.AddTransient<IBlobCacheService, BlobCacheService>(provider => new PublicBlobCacheService(
            provider.GetRequiredService<IPublicBlobStorageService>(),
            provider.GetRequiredService<ILogger<PublicBlobCacheService>>()
        ));
        services.AddTransient<IPublicBlobCacheService, PublicBlobCacheService>();
        services.AddSingleton<IMemoryCacheService>(provider =>
        {
            var memoryCacheConfig = configuration.GetSection("MemoryCache");
            var maxCacheSizeMb = memoryCacheConfig.GetValue<int>("MaxCacheSizeMb");
            var expirationScanFrequencySeconds = memoryCacheConfig.GetValue<int>("ExpirationScanFrequencySeconds");
            return new MemoryCacheService(
                new MemoryCache(
                    new MemoryCacheOptions
                    {
                        SizeLimit = maxCacheSizeMb * 1000000,
                        ExpirationScanFrequency = TimeSpan.FromSeconds(expirationScanFrequencySeconds),
                    }
                ),
                provider.GetRequiredService<ILogger<MemoryCacheService>>()
            );
        });
        services.AddTransient<IFilterRepository, FilterRepository>();
        services.AddTransient<IIndicatorRepository, IndicatorRepository>();
        services.AddTransient<IDataGuidanceService, DataGuidanceService>();
        services.AddTransient<IDataSetFileService, DataSetFileService>();
        services.AddTransient<IPublicationCacheService, PublicationCacheService>();
        services.AddTransient<IPublicationRepository, PublicationRepository>();
        services.AddTransient<IPublicationService, PublicationService>();
        services.AddTransient<IPublicationsService, PublicationsService>();
        services.AddTransient<ITimePeriodService, TimePeriodService>();
        services.AddTransient<IDataGuidanceDataSetService, DataGuidanceDataSetService>();
        services.AddTransient<IFootnoteRepository, FootnoteRepository>();
        services.AddTransient<IMethodologyImageService, MethodologyImageService>();
        services.AddTransient<IMethodologyService, MethodologyService>();
        services.AddTransient<IMethodologyRepository, MethodologyRepository>();
        services.AddTransient<IMethodologyVersionRepository, MethodologyVersionRepository>();
        services.AddTransient<IMethodologyCacheService, MethodologyCacheService>();
        services.AddTransient<IReleaseCacheService, ReleaseCacheService>();
        services.AddTransient<IReleaseRepository, ReleaseRepository>();
        services.AddTransient<IReleaseVersionRepository, ReleaseVersionRepository>();
        services.AddTransient<IReleaseService, ReleaseService>();
        services.AddTransient<IReleaseFileRepository, ReleaseFileRepository>();
        services.AddTransient<IReleaseFileService, ReleaseFileService>();
        services.AddTransient<IReleaseFileBlobService, PublicReleaseFileBlobService>();
        services.AddTransient<IReleaseDataFileRepository, ReleaseDataFileRepository>();
        services.AddTransient<IDataGuidanceFileWriter, DataGuidanceFileWriter>();
        services.AddTransient<IGlossaryCacheService, GlossaryCacheService>();
        services.AddTransient<IGlossaryService, GlossaryService>();
        services.AddTransient<IThemeService, ThemeService>();
        services.AddTransient<IPublicationMethodologiesService, PublicationMethodologiesService>();
        services.AddTransient<IPublicationReleasesService, PublicationReleasesService>();
        services.AddTransient<IPublicationsSitemapService, PublicationsSitemapService>();
        services.AddTransient<IRedirectsCacheService, RedirectsCacheService>();
        services.AddTransient<IRedirectsService, RedirectsService>();
        services.AddTransient<IRelatedInformationService, RelatedInformationService>();
        services.AddTransient<IReleaseContentService, ReleaseContentService>();
        services.AddTransient<IReleaseDataContentService, ReleaseDataContentService>();
        services.AddTransient<IReleaseSearchableDocumentsService, ReleaseSearchableDocumentsService>();
        services.AddTransient<IReleaseVersionsService, ReleaseVersionsService>();
        services.AddTransient<IReleaseUpdatesService, ReleaseUpdatesService>();
        services.AddTransient<IEducationInNumbersService, EducationInNumbersService>();

        services.AddAnalytics(configuration);

        services.AddSingleton<DateTimeProvider>();

        StartupSecurityConfiguration.ConfigureAuthorizationPolicies(services);
        StartupSecurityConfiguration.ConfigureResourceBasedAuthorization(services);

        AddPersistenceHelper<ContentDbContext>(services);
        AddPersistenceHelper<StatisticsDbContext>(services);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> _)
    {
        // Enable caching and register any caching services
        CacheAspect.Enabled = true;

        // Register the MemoryCacheService only if the Memory Caching is enabled.
        var memoryCacheConfig = configuration.GetSection("MemoryCache");
        if (memoryCacheConfig.GetValue("Enabled", false))
        {
            MemoryCacheAttribute.SetOverrideConfiguration(memoryCacheConfig.GetSection("Overrides"));
            MemoryCacheAttribute.AddService("default", app.ApplicationServices.GetService<IMemoryCacheService>()!);
        }

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            // CORS config for dev/test/etc. environments set in IaC config
            app.UseCors(options => options.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader());
        }
        else
        {
            app.UseHttpsRedirection();
            app.UseHsts(hsts => hsts.MaxAge(365).IncludeSubdomains());
        }

        var rewriteOptions = new RewriteOptions().Add(new LowercasePathRule());

        if (configuration.GetValue<bool>("enableSwagger"))
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Content API V1");
                c.RoutePrefix = "docs";
            });

            rewriteOptions.AddRedirect("^$", "docs");
        }

        app.UseRewriter(rewriteOptions);

        app.UseMiddleware(typeof(SeoSecurityHeaderMiddleware));
        app.UseMvc();
        app.UseHealthChecks("/api/health");

        app.UseResponseCompression();

        app.ServerFeatures.Get<IServerAddressesFeature>()
            ?.Addresses.ForEach(address => Console.WriteLine($"Server listening on address: {address}"));
    }
}
