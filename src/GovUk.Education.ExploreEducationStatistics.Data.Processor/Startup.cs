﻿using System;
using AutoMapper;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FileStorageService = GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.FileStorageService;
using IFileStorageService = GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces.IFileStorageService;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var serviceProvider = builder.Services
                .AddAutoMapper(typeof(Startup).Assembly)
                .AddSingleton<IBlobStorageService, BlobStorageService>(
                    provider =>
                    {
                        var connectionString = GetConfigurationValue(provider, "CoreStorage");

                        var blobStorageService = new BlobStorageService(
                            connectionString,
                            new BlobServiceClient(connectionString),
                            provider.GetRequiredService<ILogger<IBlobStorageService>>()
                        );
                        return blobStorageService;
                    })
                .AddSingleton<IFileStorageService, FileStorageService>()
                .AddTransient<IFileImportService, FileImportService>()
                .AddTransient<IImporterService, ImporterService>()
                .AddTransient<ISplitFileService, SplitFileService>()
                .AddTransient<ImporterFilterService>()
                .AddTransient<ImporterLocationService>()
                .AddTransient<IImporterMetaService, ImporterMetaService>()
                .AddTransient<IReleaseProcessorService, ReleaseProcessorService>()
                .AddTransient<ImporterMemoryCache>()
                .AddTransient<ITableStorageService, TableStorageService>(provider =>
                    new TableStorageService(GetConfigurationValue(provider, "CoreStorage")))
                .AddTransient<IBatchService, BatchService>()
                .AddTransient<IImportStatusService, ImportStatusService>()
                .AddSingleton<IValidatorService, ValidatorService>()
                .AddSingleton<IDataArchiveService, DataArchiveService>()
                .AddSingleton<IFileTypeService, FileTypeService>()
                .AddSingleton<IGuidGenerator, SequentialGuidGenerator>()
                .AddSingleton<IProcessorService, ProcessorService>()
                .BuildServiceProvider();

            ImportRecoveryHandler.CheckIncompleteImports(GetConfigurationValue(serviceProvider, "CoreStorage"));
        }

        private static string GetConfigurationValue(IServiceProvider provider, string key)
        {
            var configuration = provider.GetService<IConfiguration>();
            return configuration.GetValue<string>(key);
        }
    }
}