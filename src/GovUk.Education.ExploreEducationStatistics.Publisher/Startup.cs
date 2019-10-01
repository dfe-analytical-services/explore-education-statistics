using System;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FileStorageService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.FileStorageService;
using FileStorageServiceContentModel = GovUk.Education.ExploreEducationStatistics.Content.Model.Services.FileStorageService;
using IFileStorageService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IFileStorageService;
using IFileStorageServiceContentModel = GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces.IFileStorageService;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Publisher
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var contentDatabaseConnection = ConnectionUtils.GetConnectionString("ContentDb",
                $"{ConnectionUtils.ConnectionTypeValues[ConnectionUtils.ConnectionTypes.AZURE_SQL]}");
            
            builder.Services
                .AddAutoMapper(typeof(Startup).Assembly)
                .AddMemoryCache()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(contentDatabaseConnection))
                .AddScoped<IFileStorageService, FileStorageService>()
                .AddScoped<IFileStorageServiceContentModel, FileStorageServiceContentModel>()
                .AddScoped<IPublishingService, PublishingService>()
                .AddScoped<IContentCacheGenerationService, ContentCacheGenerationService>()
                .AddScoped<IContentService, ContentService>()
                .AddScoped<IReleaseService, ReleaseService>()
                .AddScoped<IPublicationService, PublicationService>()
                .AddScoped<IDownloadService, DownloadService>()
                .AddScoped<IMethodologyService, MethodologyService>()
                .BuildServiceProvider();
        }
    }
}