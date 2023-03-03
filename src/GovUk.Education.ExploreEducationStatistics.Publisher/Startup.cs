#nullable enable
using System;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using IContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IMethodologyService;
using ContentMethodologyService = GovUk.Education.ExploreEducationStatistics.Content.Services.MethodologyService;
using IContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IPublicationService;
using ContentPublicationService = GovUk.Education.ExploreEducationStatistics.Content.Services.PublicationService;
using IContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.IReleaseService;
using ContentReleaseService = GovUk.Education.ExploreEducationStatistics.Content.Services.ReleaseService;

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

                .AddScoped<IPublishingService, PublishingService>(provider =>
                    new PublishingService(
                        publicStorageConnectionString: GetConfigurationValue(provider, "PublicStorage"),
                        privateBlobStorageService: GetBlobStorageService(provider, "CoreStorage"),
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                        methodologyService: provider.GetRequiredService<IMethodologyService>(),
                        publicationRepository: provider.GetRequiredService<IPublicationRepository>(),
                        releaseService: provider.GetRequiredService<IReleaseService>(),
                        logger: provider.GetRequiredService<ILogger<PublishingService>>()))
                .AddScoped<IContentService, ContentService>(provider =>
                    new ContentService(
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                        privateBlobCacheService: GetBlobCacheService(provider, "CoreStorage"),
                        publicBlobCacheService: GetBlobCacheService(provider, "PublicStorage"),
                        releaseCacheService: provider.GetRequiredService<IReleaseCacheService>(),
                        releaseService: provider.GetRequiredService<IReleaseService>(),
                        methodologyCacheService: provider.GetRequiredService<IMethodologyCacheService>(),
                        publicationCacheService: provider.GetRequiredService<IPublicationCacheService>()))
                .AddScoped<IReleaseService, ReleaseService>()
                .AddScoped<ITableStorageService, TableStorageService>(provider =>
                    new TableStorageService(
                        GetConfigurationValue(provider, "PublisherStorage"),
                        new StorageInstanceCreationUtil()))
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
                .AddScoped<IPermalinkMigrationService, PermalinkMigrationService>(provider =>
                    new PermalinkMigrationService(
                        contentDbContext: provider.GetRequiredService<ContentDbContext>(),
                        blobServiceClient: new BlobServiceClient(
                            GetConfigurationValue(provider, "PublicStorage")),
                        storageQueueService: new StorageQueueService(
                            GetConfigurationValue(provider, "PublisherStorage"),
                            new StorageInstanceCreationUtil()
                        )))
                .AddScoped<IFilterRepository, FilterRepository>()
                .AddScoped<IFootnoteRepository, FootnoteRepository>()
                .AddScoped<IIndicatorRepository, IndicatorRepository>()
                .AddScoped<IPublishingCompletionService, PublishingCompletionService>()
                .AddScoped<IPublicationRepository, PublicationRepository>()
                .AddScoped<IReleaseRepository, ReleaseRepository>();

            AddPersistenceHelper<ContentDbContext>(builder.Services);
            AddPersistenceHelper<StatisticsDbContext>(builder.Services);
        }

        private static IBlobCacheService GetBlobCacheService(IServiceProvider provider, string connectionStringKey)
        {
            return new BlobCacheService(
                blobStorageService: GetBlobStorageService(provider, connectionStringKey),
                logger: provider.GetRequiredService<ILogger<BlobCacheService>>());
        }

        private static IBlobStorageService GetBlobStorageService(IServiceProvider provider, string connectionStringKey)
        {
            var connectionString = GetConfigurationValue(provider, connectionStringKey);
            return new BlobStorageService(
                connectionString,
                new BlobServiceClient(connectionString),
                provider.GetRequiredService<ILogger<BlobStorageService>>(),
                new StorageInstanceCreationUtil());
        }

        private static string GetConfigurationValue(IServiceProvider provider, string key)
        {
            var configuration = provider.GetService<IConfiguration>();
            return configuration.GetValue<string>(key);
        }
    }
}
