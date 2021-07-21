﻿using System;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.StartupUtils;
using IPublicationService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IPublicationService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IReleaseService;
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
                .AddScoped<IPublishingService, PublishingService>(provider =>
                    new PublishingService(
                        publicStorageConnectionString: GetConfigurationValue(provider, "PublicStorage"),
                        privateBlobStorageService: GetBlobStorageService(provider, "CoreStorage"),
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                        methodologyService: provider.GetRequiredService<IMethodologyService>(),
                        publicationService: provider.GetRequiredService<IPublicationService>(),
                        releaseService: provider.GetRequiredService<IReleaseService>(),
                        zipFileService: provider.GetRequiredService<IZipFileService>(),
                        dataGuidanceFileService: provider.GetRequiredService<IDataGuidanceFileService>(),
                        logger: provider.GetRequiredService<ILogger<PublishingService>>()))
                .AddScoped<IContentService, ContentService>(provider =>
                    new ContentService(
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                        fastTrackService: provider.GetService<IFastTrackService>(),
                        downloadService: provider.GetRequiredService<IDownloadService>(),
                        releaseService: provider.GetRequiredService<IReleaseService>(),
                        publicationService: provider.GetRequiredService<IPublicationService>()
                    ))
                .AddScoped<IReleaseService, ReleaseService>(provider =>
                    new ReleaseService(
                        contentDbContext: provider.GetService<ContentDbContext>(),
                        publicStatisticsDbContext: provider.GetService<PublicStatisticsDbContext>(),
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                        methodologyService: provider.GetService<IMethodologyService>(),
                        releaseSubjectService: provider.GetService<IReleaseSubjectService>(),
                        logger: provider.GetRequiredService<ILogger<ReleaseService>>(),
                        mapper: provider.GetRequiredService<IMapper>()
                    ))
                .AddScoped<ITableStorageService, TableStorageService>(provider =>
                    new TableStorageService(GetConfigurationValue(provider, "PublisherStorage")))
                .AddScoped<IPublicationService, PublicationService>()
                .AddScoped<IDownloadService, DownloadService>()
                .AddScoped<IFastTrackService, FastTrackService>(provider =>
                    new FastTrackService(
                        contentDbContext: provider.GetService<ContentDbContext>(),
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage"),
                        tableStorageService: new TableStorageService(GetConfigurationValue(provider, "PublicStorage"))))
                .AddScoped<IMethodologyRepository, MethodologyRepository>()
                .AddScoped<IMethodologyParentRepository, MethodologyParentRepository>()
                .AddScoped<IMethodologyService, MethodologyService>()
                .AddScoped<INotificationsService, NotificationsService>(provider =>
                    new NotificationsService(
                        context: provider.GetService<ContentDbContext>(),
                        storageQueueService: new StorageQueueService(GetConfigurationValue(provider,
                            "NotificationStorage"))))
                .AddScoped<IQueueService, QueueService>(provider =>
                    new QueueService(
                        storageQueueService: new StorageQueueService(
                            storageConnectionString: GetConfigurationValue(provider, "PublisherStorage")),
                            releaseStatusService: provider.GetService<IReleaseStatusService>(),
                            logger: provider.GetRequiredService<ILogger<QueueService>>()))
                .AddScoped<IReleaseStatusService, ReleaseStatusService>()
                .AddScoped<IValidationService, ValidationService>()
                .AddScoped<IReleaseSubjectService, ReleaseSubjectService>(provider =>
                    new ReleaseSubjectService(
                        statisticsDbContext: provider.GetService<PublicStatisticsDbContext>(),
                        footnoteRepository: provider.GetService<IFootnoteRepository>()))
                .AddScoped<IFilterService, FilterService>()
                .AddScoped<IIndicatorService, IndicatorService>()
                .AddScoped<IReleaseDataFileRepository, ReleaseDataFileRepository>()
                .AddScoped<IMetaGuidanceSubjectService, MetaGuidanceSubjectService>()
                .AddScoped<IDataGuidanceFileWriter, DataGuidanceFileWriter>()
                .AddScoped<IDataGuidanceFileService, DataGuidanceFileService>(provider =>
                    new DataGuidanceFileService(
                        contentDbContext: provider.GetService<ContentDbContext>(),
                        dataGuidanceFileWriter: provider.GetService<IDataGuidanceFileWriter>(),
                        blobStorageService: GetBlobStorageService(provider, "CoreStorage")
                    ))
                .AddScoped<IFootnoteRepository, FootnoteRepository>()
                .AddScoped<IZipFileService, ZipFileService>(provider =>
                    new ZipFileService(
                        publicBlobStorageService: GetBlobStorageService(provider, "PublicStorage")
                    ));

            AddPersistenceHelper<StatisticsDbContext>(builder.Services);
            AddPersistenceHelper<PublicStatisticsDbContext>(builder.Services);
        }

        private static IBlobStorageService GetBlobStorageService(IServiceProvider provider, string connectionStringKey)
        {
            var connectionString = GetConfigurationValue(provider, connectionStringKey);
            return new BlobStorageService(
                connectionString,
                new BlobServiceClient(connectionString),
                provider.GetRequiredService<ILogger<BlobStorageService>>());
        }

        private static string GetConfigurationValue(IServiceProvider provider, string key)
        {
            var configuration = provider.GetService<IConfiguration>();
            return configuration.GetValue<string>(key);
        }
    }
}
