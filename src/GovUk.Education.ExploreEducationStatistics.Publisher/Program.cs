using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Mappings;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Configuration;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using IContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IMethodologyService;
using ContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.MethodologyService;
using IContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using ContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.PublicationService;
using IContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;
using ContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.ReleaseService;
using IMethodologyService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IMethodologyService;
using MethodologyService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.MethodologyService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IReleaseService;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.ReleaseService;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder =>
    {
        builder
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .Configure<JsonSerializerOptions>(options =>
            {
                // The 'IncludeFields' setting is necessary for the deserialization of the System.ValueTuple's used by
                // the StageReleaseContentMessage and PublishReleaseFilesMessage queue message types.
                // See https://github.com/dotnet/runtime/issues/70352
                options.IncludeFields = true;
            })
            .AddMemoryCache()
            .AddDbContext<ContentDbContext>(options =>
                options.UseSqlServer(
                    ConnectionUtils.GetAzureSqlConnectionString("ContentDb"),
                    providerOptions => providerOptions.EnableCustomRetryOnFailure()))
            .AddDbContext<StatisticsDbContext>(options =>
                options.UseSqlServer(
                    ConnectionUtils.GetAzureSqlConnectionString("StatisticsDb"),
                    providerOptions => providerOptions.EnableCustomRetryOnFailure()))
            .AddSingleton<IFileStorageService, FileStorageService>(provider =>
                new FileStorageService(hostContext.Configuration.GetValue<string>("PublisherStorage")))
            .AddScoped<IPublishingService, PublishingService>()
            .AddScoped<IPublicBlobStorageService, PublicBlobStorageService>()
            .AddScoped<IPrivateBlobStorageService, PrivateBlobStorageService>()
            .AddScoped<IContentService, ContentService>(provider =>
                new ContentService(
                    publicBlobStorageService: provider.GetRequiredService<IPublicBlobStorageService>(),
                    privateBlobCacheService: new BlobCacheService(
                        provider.GetRequiredService<IPrivateBlobStorageService>(),
                        provider.GetRequiredService<ILogger<BlobCacheService>>()),
                    publicBlobCacheService: new BlobCacheService(
                        provider.GetRequiredService<IPublicBlobStorageService>(),
                        provider.GetRequiredService<ILogger<BlobCacheService>>()),
                    releaseCacheService: provider.GetRequiredService<IReleaseCacheService>(),
                    releaseService: provider.GetRequiredService<IReleaseService>(),
                    methodologyCacheService: provider.GetRequiredService<IMethodologyCacheService>(),
                    publicationCacheService: provider.GetRequiredService<IPublicationCacheService>()))
            .AddScoped<IReleaseService, ReleaseService>()
            .AddScoped<IPublisherTableStorageService, PublisherTableStorageService>()
            .AddScoped<IMethodologyVersionRepository, MethodologyVersionRepository>()
            .AddScoped<IMethodologyRepository, MethodologyRepository>()
            .AddScoped<IMethodologyService, MethodologyService>()
            .AddScoped<INotificationsService, NotificationsService>(provider =>
                new NotificationsService(
                    context: provider.GetRequiredService<ContentDbContext>(),
                    storageQueueService: new StorageQueueService(
                        hostContext.Configuration.GetValue<string>("NotificationStorage"),
                        new StorageInstanceCreationUtil())))
            .AddScoped<IQueueService, QueueService>(provider =>
                new QueueService(
                    storageQueueService: new StorageQueueService(
                        hostContext.Configuration.GetValue<string>("PublisherStorage"),
                        new StorageInstanceCreationUtil()
                    ),
                    releasePublishingStatusService: provider.GetRequiredService<IReleasePublishingStatusService>(),
                    logger: provider.GetRequiredService<ILogger<QueueService>>()))
            .AddScoped<IReleasePublishingStatusService, ReleasePublishingStatusService>()
            .AddScoped<IValidationService, ValidationService>()
            .AddScoped<IFilterRepository, FilterRepository>()
            .AddScoped<IFootnoteRepository, FootnoteRepository>()
            .AddScoped<IIndicatorRepository, IndicatorRepository>()
            .AddScoped<IPublishingCompletionService, PublishingCompletionService>()
            .AddScoped<IPublicationRepository, PublicationRepository>()
            .AddScoped<IReleaseVersionRepository, ReleaseVersionRepository>()
            .AddScoped<IRedirectsCacheService, RedirectsCacheService>()
            .AddScoped<IRedirectsService, RedirectsService>()
            .Configure<AppSettingOptions>(hostContext.Configuration.GetSection(AppSettingOptions.AppSettings));

        // TODO EES-3510 These services from the Content.Services namespace are used to update cached resources.
        // EES-3528 plans to send a request to the Content API to update its cached resources instead of this
        // being done from Publisher directly, and so these DI dependencies should eventually be removed.
        services
            .AddAutoMapper(typeof(MappingProfiles))
            // UserService is added to satisfy the IUserService dependency in Content.Services.ReleaseService
            // even though it is not used
            .AddScoped<IUserService, UserService>(_ => new UserService(null!, null!))
            .AddScoped<IContentMethodologyService, ContentMethodologyService>()
            .AddScoped<IMethodologyCacheService, MethodologyCacheService>()
            .AddScoped<IContentPublicationService, ContentPublicationService>()
            .AddScoped<IPublicationCacheService, PublicationCacheService>()
            .AddScoped<IContentReleaseService, ContentReleaseService>()
            .AddScoped<IReleaseFileRepository, ReleaseFileRepository>()
            .AddScoped<IReleaseCacheService, ReleaseCacheService>();

        StartupUtils.AddPersistenceHelper<ContentDbContext>(services);
        StartupUtils.AddPersistenceHelper<StatisticsDbContext>(services);
    })
    .Build();

EnableCaching();

await host.RunAsync();
return;

void EnableCaching()
{
    // Enable caching and register any caching services
    CacheAspect.Enabled = true;
    BlobCacheAttribute.AddService("public", new BlobCacheService(
        host.Services.GetRequiredService<IPublicBlobStorageService>(),
        host.Services.GetRequiredService<ILogger<BlobCacheService>>()));
}
