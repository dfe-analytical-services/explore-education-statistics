using System;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FileStorageService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.FileStorageService;
using IFileStorageService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IFileStorageService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IReleaseService;
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
                .AddSingleton<IFileStorageService, FileStorageService>(
                    provider =>
                    {
                        var privateStorageConnectionString = GetConfigurationValue(provider, "CoreStorage");
                        var publicStorageConnectionString = GetConfigurationValue(provider, "PublicStorage");
                        var publisherStorageConnectionString = GetConfigurationValue(provider, "PublisherStorage");

                        var privateBlobStorageService = new BlobStorageService(
                            privateStorageConnectionString,
                            new BlobServiceClient(privateStorageConnectionString),
                            provider.GetRequiredService<ILogger<BlobStorageService>>());

                        var publicBlobStorageService = new BlobStorageService(
                            publicStorageConnectionString,
                            new BlobServiceClient(publicStorageConnectionString),
                            provider.GetRequiredService<ILogger<BlobStorageService>>());

                        var publisherBlobStorageService = new BlobStorageService(
                            publicStorageConnectionString,
                            new BlobServiceClient(publisherStorageConnectionString),
                            provider.GetRequiredService<ILogger<BlobStorageService>>());

                        return new FileStorageService(
                            privateBlobStorageService: privateBlobStorageService,
                            publicBlobStorageService: publicBlobStorageService,
                            publicStorageConnectionString: publicStorageConnectionString,
                            publisherStorageConnectionString: publisherStorageConnectionString,
                            logger: provider.GetRequiredService<ILogger<FileStorageService>>());
                    })
                .AddScoped<IPublishingService, PublishingService>()
                .AddScoped<IContentService, ContentService>()
                .AddScoped<IReleaseService, ReleaseService>()
                .AddScoped<ITableStorageService, TableStorageService>(provider =>
                    new TableStorageService(GetConfigurationValue(provider, "PublisherStorage")))
                .AddScoped<IPublicationService, PublicationService>()
                .AddScoped<IDownloadService, DownloadService>()
                .AddScoped<IFastTrackService, FastTrackService>(provider =>
                    new FastTrackService(provider.GetService<ContentDbContext>(),
                        provider.GetService<IFileStorageService>(),
                        new TableStorageService(GetConfigurationValue(provider, "PublicStorage"))))
                .AddScoped<IMethodologyService, MethodologyService>()
                .AddScoped<INotificationsService, NotificationsService>(provider =>
                    new NotificationsService(provider.GetService<ContentDbContext>(),
                        new StorageQueueService(GetConfigurationValue(provider, "NotificationStorage"))))
                .AddScoped<IQueueService, QueueService>(provider => new QueueService(
                    new StorageQueueService(GetConfigurationValue(provider, "PublisherStorage")),
                    provider.GetService<IReleaseStatusService>(),
                    provider.GetRequiredService<ILogger<QueueService>>()))
                .AddScoped<IReleaseStatusService, ReleaseStatusService>()
                .AddScoped<IValidationService, ValidationService>()
                .AddScoped<IReleaseSubjectService, ReleaseSubjectService>()
                .AddScoped<IFootnoteService, FootnoteService>();
        }

        private static string GetConfigurationValue(IServiceProvider provider, string key)
        {
            var configuration = provider.GetService<IConfiguration>();
            return configuration.GetValue<string>(key);
        }
    }
}