﻿using System;
using AutoMapper;
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
                .AddScoped<IFileStorageService, FileStorageService>()
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
                .AddScoped<INotificationsService, NotificationsService>()
                .AddScoped<IQueueService, QueueService>()
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