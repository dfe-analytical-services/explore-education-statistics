using System;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services
                .AddDbContext<ContentDbContext>(options =>
                    options.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("ContentDb"),
                        providerOptions => providerOptions.EnableCustomRetryOnFailure()))
                .AddSingleton<IBlobStorageService, BlobStorageService>(
                    provider =>
                    {
                        var connectionString = GetConfigurationValue(provider, "CoreStorage");

                        var blobStorageService = new BlobStorageService(
                            connectionString,
                            new BlobServiceClient(connectionString),
                            provider.GetRequiredService<ILogger<BlobStorageService>>(),
                            new StorageInstanceCreationUtil()
                        );
                        return blobStorageService;
                    })
                .AddSingleton<IStorageQueueService, StorageQueueService>(provider =>
                    new StorageQueueService(
                        GetConfigurationValue(provider, "CoreStorage"),
                        new StorageInstanceCreationUtil()))
                .AddTransient<IFileImportService, FileImportService>()
                .AddTransient<IImporterService, ImporterService>()
                .AddTransient<ISplitFileService, SplitFileService>()
                .AddTransient<ImporterFilterService>()
                .AddTransient<ImporterLocationService>()
                .AddTransient<IImporterMetaService, ImporterMetaService>()
                .AddTransient<ImporterFilterCache>()
                .AddTransient<IBatchService, BatchService>()
                .AddTransient<IDataImportService, DataImportService>()
                .AddTransient<IValidatorService, ValidatorService>()
                .AddSingleton<IDataArchiveService, DataArchiveService>()
                .AddSingleton<IFileTypeService, FileTypeService>()
                .AddSingleton<IGuidGenerator, SequentialGuidGenerator>()
                .AddTransient<IProcessorService, ProcessorService>()
                .AddSingleton<IDatabaseHelper, DatabaseHelper>()
                .AddSingleton<IImporterLocationCache, ImporterLocationCache>()
                .AddSingleton<IDbContextSupplier, DbContextSupplier>()
                .BuildServiceProvider();
            HandleRestart(serviceProvider);
        }

        private static void HandleRestart(IServiceProvider serviceProvider)
        {
            var storageQueueService = serviceProvider.GetRequiredService<IStorageQueueService>();
            storageQueueService.Clear(ImportsAvailableQueue).Wait();
            storageQueueService.Clear(ImportsPendingQueue).Wait();
            storageQueueService.Clear(RestartImportsQueue).Wait();

            storageQueueService.AddMessage(RestartImportsQueue, new RestartImportsMessage());
        }

        private static string GetConfigurationValue(IServiceProvider provider, string key)
        {
            var configuration = provider.GetService<IConfiguration>();
            return configuration.GetValue<string>(key);
        }
    }
}
