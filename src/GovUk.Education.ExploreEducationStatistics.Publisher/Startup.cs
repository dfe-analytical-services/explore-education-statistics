using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FileStorageService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.FileStorageService;
using IFileStorageService = GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces.IFileStorageService;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Publisher
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var contentDatabaseConnection = GetSqlAzureConnectionString("ContentDb");
            
            builder.Services
                .AddMemoryCache()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(contentDatabaseConnection))
                .AddTransient<IFileStorageService, FileStorageService>()
                .AddTransient<IPublishingService, PublishingService>()
                .AddTransient<IContentCacheGenerationService, ContentCacheGenerationService>()
                .AddTransient<IContentService, ContentService>()
                .AddTransient<IDownloadService, DownloadService>()
                .AddTransient<IMethodologyService, MethodologyService>()
                .BuildServiceProvider();
        }
        
        private static string GetSqlAzureConnectionString(string name)
        {
            // Attempt to get a connection string defined for running locally.
            // Settings in the local.settings.json file are only used by Functions tools when running locally.
            var connectionString =
                Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(connectionString))
            {
                // Get the connection string from the Azure Functions App using the naming convention for type SQLAzure.
                connectionString = Environment.GetEnvironmentVariable(
                    $"SQLAZURECONNSTR_{name}",
                    EnvironmentVariableTarget.Process);
            }

            return connectionString;
        }
    }
}