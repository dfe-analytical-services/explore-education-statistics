#nullable enable
using System;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
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
using FileStorageService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.FileStorageService;
using IFileStorageService =
    GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IFileStorageService;
using IMethodologyService =
    GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IMethodologyService;
using IPublicationService =
    GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IPublicationService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IReleaseService;
using MethodologyService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.MethodologyService;
using PublicationService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.PublicationService;
using ReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.ReleaseService;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Publisher
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddAutoMapper(typeof(Startup).Assembly)
                .AddMemoryCache()
                .AddDbContext<ContentDbContext>(options =>
                    options.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("ContentDb")))
                .AddDbContext<StatisticsDbContext>(options =>
                    options.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("StatisticsDb")))
                .AddDbContext<PublicStatisticsDbContext>(options =>
                    options.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("PublicStatisticsDb")))
                .AddSingleton<IFileStorageService, FileStorageService>(provider =>
                    new FileStorageService(GetConfigurationValue(provider, "PublisherStorage")))
                .AddScoped(provider => GetBlobCacheService(provider, "PublicStorage"))
                .AddScoped<IPublishingService, PublishingService>(provider =>
                    new PublishingService(
                        publicStorageConnectionString: GetConfigurationValue(provider, "PublicStorage"),
                        privateBlobStorageService: GetBlobStorageService(provider, "CoreStorage"),
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                        publicBlobCacheService: GetBlobCacheService(provider, "PublicStorage"),
                        methodologyService: provider.GetRequiredService<IMethodologyService>(),
                        publicationService: provider.GetRequiredService<IPublicationService>(),
                        releaseService: provider.GetRequiredService<IReleaseService>(),
                        contentDbContext: provider.GetService<ContentDbContext>(),
                        logger: provider.GetRequiredService<ILogger<PublishingService>>()))
                .AddScoped<IContentService, ContentService>(provider =>
                    new ContentService(
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                        privateBlobCacheService: GetBlobCacheService(provider, "CoreStorage"),
                        publicBlobCacheService: GetBlobCacheService(provider, "PublicStorage"),
                        releaseService: provider.GetRequiredService<IReleaseService>(),
                        publicationService: provider.GetRequiredService<IPublicationService>()
                    ))
                .AddScoped<IReleaseService, ReleaseService>(provider =>
                    new ReleaseService(
                        contentDbContext: provider.GetService<ContentDbContext>(),
                        statisticsDbContext: provider.GetService<StatisticsDbContext>(),
                        publicStatisticsDbContext: provider.GetService<PublicStatisticsDbContext>(),
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                        methodologyService: provider.GetService<IMethodologyService>(),
                        releaseSubjectRepository: provider.GetService<IReleaseSubjectRepository>(),
                        logger: provider.GetRequiredService<ILogger<ReleaseService>>(),
                        mapper: provider.GetRequiredService<IMapper>()
                    ))
                .AddScoped<ITableStorageService, TableStorageService>(provider =>
                    new TableStorageService(
                        GetConfigurationValue(provider, "PublisherStorage"),
                        new StorageInstanceCreationUtil()))
                .AddScoped<IPublicationService, PublicationService>()
                .AddScoped<IMethodologyVersionRepository, MethodologyVersionRepository>()
                .AddScoped<IMethodologyRepository, MethodologyRepository>()
                .AddScoped<IMethodologyService, MethodologyService>()
                .AddScoped<INotificationsService, NotificationsService>(provider =>
                    new NotificationsService(
                        context: provider.GetService<ContentDbContext>(),
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
                .AddScoped<IReleaseSubjectRepository, ReleaseSubjectRepository>(provider =>
                    new ReleaseSubjectRepository(
                        statisticsDbContext: provider.GetService<PublicStatisticsDbContext>(),
                        footnoteRepository: new FootnoteRepository(provider.GetService<PublicStatisticsDbContext>())
                    ))
                .AddScoped<IFilterRepository, FilterRepository>()
                .AddScoped<IFootnoteRepository, FootnoteRepository>()
                .AddScoped<IIndicatorRepository, IndicatorRepository>()

                // Service temporarily added to migrate files in EES-3547
                // TODO Remove in EES-3552
                .AddScoped<IFileMigrationService, FileMigrationService>(provider =>
                    new FileMigrationService(
                        contentDbContext: provider.GetService<ContentDbContext>(),
                        provider.GetService<IPersistenceHelper<ContentDbContext>>(),
                        privateBlobStorageService: GetBlobStorageService(provider, "CoreStorage")
                    ));

            AddPersistenceHelper<ContentDbContext>(builder.Services);
            AddPersistenceHelper<StatisticsDbContext>(builder.Services);
            AddPersistenceHelper<PublicStatisticsDbContext>(builder.Services);
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
