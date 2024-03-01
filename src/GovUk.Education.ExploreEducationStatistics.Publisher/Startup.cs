#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
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
using GovUk.Education.ExploreEducationStatistics.Publisher;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using ContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.MethodologyService;
using ContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.PublicationService;
using ContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.ReleaseService;
using IContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IMethodologyService;
using IContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using IContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;
using IMethodologyService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IMethodologyService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IReleaseService;
using MethodologyService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.MethodologyService;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.ReleaseService;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Publisher
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
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
                    new FileStorageService(GetConfigurationValue(provider, "PublisherStorage")))

                // TODO EES-3510 These services from the Content.Services namespace are used to update cached resources.
                // EES-3528 plans to send a request to the Content API to update its cached resources instead of this
                // being done from Publisher directly, and so these DI dependencies should eventually be removed.
                .AddAutoMapper(typeof(MappingProfiles))
                .AddScoped<IContentMethodologyService, ContentMethodologyService>()
                .AddScoped<IMethodologyCacheService, MethodologyCacheService>()
                .AddScoped<IContentPublicationService, ContentPublicationService>()
                .AddScoped<IPublicationCacheService, PublicationCacheService>()
                .AddScoped<IContentReleaseService, ContentReleaseService>()
                .AddScoped<IReleaseFileRepository, ReleaseFileRepository>()
                .AddScoped<IReleaseCacheService, ReleaseCacheService>()
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
                .AddScoped<Admin.Services.Interfaces.IPublicationService, Admin.Services.PublicationService>()
                .AddScoped<IPublisherTableStorageService, PublisherTableStorageService>()
                .AddScoped<IMethodologyVersionRepository, MethodologyVersionRepository>()
                .AddScoped<IMethodologyRepository, MethodologyRepository>()
                .AddScoped<IMethodologyService, MethodologyService>()
                .AddScoped<INotificationsService, NotificationsService>(provider =>
                    new NotificationsService(
                        context: provider.GetRequiredService<ContentDbContext>(),
                        storageQueueService: new StorageQueueService(
                            GetConfigurationValue(provider,
                                "NotificationStorage"),
                            new StorageInstanceCreationUtil())))
                .AddScoped<IQueueService, QueueService>(provider =>
                    new QueueService(
                        storageQueueService: new StorageQueueService(
                            GetConfigurationValue(provider, "PublisherStorage"),
                            new StorageInstanceCreationUtil()
                        ),
                        releasePublishingStatusService: provider.GetService<IReleasePublishingStatusService>(),
                        logger: provider.GetRequiredService<ILogger<QueueService>>()))
                .AddScoped<IReleasePublishingStatusService, ReleasePublishingStatusService>()
                .AddScoped<IValidationService, ValidationService>()
                .AddScoped<IFilterRepository, FilterRepository>()
                .AddScoped<IFootnoteRepository, FootnoteRepository>()
                .AddScoped<IIndicatorRepository, IndicatorRepository>()
                .AddScoped<IPublishingCompletionService, PublishingCompletionService>()
                .AddScoped<Admin.Services.Interfaces.IPublicationReleaseSeriesViewService, Admin.Services.PublicationReleaseSeriesViewService>()
                .AddScoped<IPublicationRepository, PublicationRepository>()
                .AddScoped<IReleaseRepository, ReleaseRepository>()
                .AddScoped<IRedirectsCacheService, RedirectsCacheService>()
                .AddScoped<IRedirectsService, RedirectsService>();

            AddPersistenceHelper<ContentDbContext>(builder.Services);
            AddPersistenceHelper<StatisticsDbContext>(builder.Services);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            var binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var rootDir = Path.GetFullPath(Path.Combine(binDir!, ".."));

            builder.ConfigurationBuilder
                .AddJsonFile($"{rootDir}/appsettings.Local.json", optional: true, reloadOnChange: false);
        }

        private static string GetConfigurationValue(IServiceProvider provider, string key)
        {
            var configuration = provider.GetService<IConfiguration>();
            return configuration.GetValue<string>(key);
        }
    }
}
