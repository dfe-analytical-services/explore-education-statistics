using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Configuration;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ProcessorQueues;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder =>
    {
        builder
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .AddDbContext<ContentDbContext>(options =>
                options.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("ContentDb"),
                    providerOptions => providerOptions.EnableCustomRetryOnFailure()))
            .AddSingleton<IPrivateBlobStorageService, PrivateBlobStorageService>()
            .AddSingleton<IStorageQueueService, StorageQueueService>(_ =>
                new StorageQueueService(hostContext.Configuration.GetValue<string>("CoreStorage"),
                    new StorageInstanceCreationUtil()))
            .AddTransient<IFileImportService, FileImportService>()
            .AddTransient<IImporterService, ImporterService>()
            .AddTransient<ImporterLocationService>()
            .AddTransient<IImporterMetaService, ImporterMetaService>()
            .AddTransient<IDataImportService, DataImportService>()
            .AddTransient<IValidatorService, ValidatorService>()
            .AddSingleton<IDataArchiveService, DataArchiveService>()
            .AddSingleton<IFileTypeService, FileTypeService>()
            .AddSingleton<IGuidGenerator, SequentialGuidGenerator>()
            .AddTransient<IProcessorService, ProcessorService>()
            .AddSingleton<IDatabaseHelper, DatabaseHelper>()
            .AddSingleton<IImporterLocationCache, ImporterLocationCache>()
            .AddSingleton<IDbContextSupplier, DbContextSupplier>()
            .Configure<AppSettingOptions>(hostContext.Configuration.GetSection(AppSettingOptions.AppSettings));
    })
    .Build();

LoadLocationCache();
await RestartImports();
await host.RunAsync();
return;

void LoadLocationCache()
{
    var dbContextSupplier = host.Services.GetRequiredService<IDbContextSupplier>();
    var locationCache = host.Services.GetRequiredService<IImporterLocationCache>();
    var statisticsDbContext = dbContextSupplier.CreateDbContext<StatisticsDbContext>();
    locationCache.LoadLocations(statisticsDbContext);
}

async Task RestartImports()
{
    var storageQueueService = host.Services.GetRequiredService<IStorageQueueService>();
    await storageQueueService.Clear(ImportsPendingQueue);
    await storageQueueService.Clear(RestartImportsQueue);
    storageQueueService.AddMessage(RestartImportsQueue, new RestartImportsMessage());
}
