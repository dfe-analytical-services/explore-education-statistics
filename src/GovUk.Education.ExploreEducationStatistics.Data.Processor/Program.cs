using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Options;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    .ConfigureLogging(logging =>
    {
        // TODO EES-5013 Why can't this be controlled through application settings?
        logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights()
            .AddDbContext<ContentDbContext>(options =>
                options.UseSqlServer(ConnectionUtils.GetAzureSqlConnectionString("ContentDb"),
                    providerOptions => providerOptions.EnableCustomRetryOnFailure()))
            .AddSingleton<IBlobSasService, BlobSasService>()
            .AddSingleton<IPrivateBlobStorageService, PrivateBlobStorageService>(provider =>
                new PrivateBlobStorageService(
                    connectionString: provider
                        .GetRequiredService<IOptions<AppOptions>>()
                        .Value
                        .PrivateStorageConnectionString,
                    logger: provider.GetRequiredService<ILogger<IBlobStorageService>>(),
                    sasService: provider.GetRequiredService<IBlobSasService>())
            )
            .AddTransient<IFileImportService, FileImportService>()
            .AddTransient<IImporterService, ImporterService>()
            .AddTransient<ImporterLocationService>()
            .AddTransient<IImporterMetaService, ImporterMetaService>()
            .AddTransient<IDataImportService, DataImportService>()
            .AddTransient<IValidatorService, ValidatorService>()
            .AddSingleton<IFileTypeService, FileTypeService>()
            .AddSingleton<IGuidGenerator, SequentialGuidGenerator>()
            .AddTransient<IProcessorService, ProcessorService>()
            .AddSingleton<IDatabaseHelper, DatabaseHelper>()
            .AddSingleton<IImporterLocationCache, ImporterLocationCache>()
            .AddSingleton<IDbContextSupplier, DbContextSupplier>()
            .Configure<AppOptions>(hostContext.Configuration.GetSection(AppOptions.Section));
    })
    .Build();

LoadLocationCache();
await ClearQueues();
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

async Task ClearQueues()
{
    var config = host.Services.GetRequiredService<IOptions<AppOptions>>().Value;
    var connectionString = config.PrivateStorageConnectionString;

    var importsPendingQueueClient = new QueueClient(connectionString, queueName: ImportsPendingQueue);
    await importsPendingQueueClient.CreateIfNotExistsAsync();
    await importsPendingQueueClient.ClearMessagesAsync();

    var restartImportsQueueClient = new QueueClient(connectionString, queueName: RestartImportsQueue);
    await restartImportsQueueClient.CreateIfNotExistsAsync();
    await restartImportsQueueClient.ClearMessagesAsync();
}

async Task RestartImports()
{
    var config = host.Services.GetRequiredService<IOptions<AppOptions>>().Value;
    var connectionString = config.PrivateStorageConnectionString;

    QueueClientOptions queueOptions =
        new() { MessageEncoding = QueueMessageEncoding.Base64 };

    var restartImportsQueueClient = new QueueClient(connectionString, queueName: RestartImportsQueue, queueOptions);
    await restartImportsQueueClient.SendMessageAsync(JsonSerializer.Serialize(new RestartImportsMessage()));
}
