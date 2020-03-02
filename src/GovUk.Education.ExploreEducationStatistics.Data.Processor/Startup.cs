using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddAutoMapper(typeof(Startup).Assembly)
                .AddTransient<IFileStorageService, FileStorageService>(s =>
                    new FileStorageService(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage")))
                .AddTransient<IFileImportService, FileImportService>()
                .AddTransient<IImporterService, ImporterService>()
                .AddTransient<ISplitFileService, SplitFileService>()
                .AddTransient<ImporterFilterService>()
                .AddTransient<ImporterLocationService>()
                .AddTransient<IImporterMetaService, ImporterMetaService>()
                .AddTransient<IReleaseProcessorService, ReleaseProcessorService>()
                .AddTransient<ImporterMemoryCache>()
                .AddTransient<ITableStorageService, TableStorageService>(s =>
                    new TableStorageService(ConnectionUtils.GetAzureStorageConnectionString("CoreStorage")))
                .AddTransient<IBatchService, BatchService>()
                .AddTransient<IImportStatusService, ImportStatusService>()
                .AddSingleton<IValidatorService, ValidatorService>()
                .AddApplicationInsightsTelemetry()
                .BuildServiceProvider();
            
            FailedImportsHandler.CheckIncompleteImports();
        }
    }
}