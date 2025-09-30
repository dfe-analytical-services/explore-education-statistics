using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Options;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Analytics.Consumer.Services.Workflow;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Consumer;

public static class ProcessorHostBuilder
{
    public static IHostBuilder ConfigureProcessorHostBuilder(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables();
            })
            .ConfigureLogging(logging =>
            {
                // TODO EES-5013 Why can't Command logging be suppressed via the logging config in application settings?
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            })
            .ConfigureServices((builder, services) =>
            {
                var configuration = builder.Configuration;

                services.AddOptions<AnalyticsOptions>()
                    .Bind(configuration.GetRequiredSection(AnalyticsOptions.Section));

                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    .AddTransient<IAnalyticsPathResolver, AnalyticsPathResolver>();

                // To be used by ConsumeAnalyticsRequestFilesFunction
                services
                    .AddTransient<IRequestFileProcessor, PublicApiTopLevelCallsProcessor>()
                    .AddTransient<IRequestFileProcessor, PublicApiPublicationCallsProcessor>()
                    .AddTransient<IRequestFileProcessor, PublicApiDataSetCallsProcessor>()
                    .AddTransient<IRequestFileProcessor, PublicApiDataSetVersionCallsProcessor>()
                    .AddTransient<IRequestFileProcessor, PublicApiQueriesProcessor>()
                    .AddTransient<IRequestFileProcessor, PublicZipDownloadsProcessor>()
                    .AddTransient<IRequestFileProcessor, PublicCsvDownloadsProcessor>()
                    .AddTransient<IRequestFileProcessor, TableToolDownloadsProcessor>()
                    .AddTransient<IRequestFileProcessor, PermalinksTableDownloadsProcessor>()
                    .AddTransient<IProcessRequestFilesWorkflow, ProcessRequestFilesWorkflow>()
                    .AddTransient<IFileAccessor, FilesystemFileAccessor>()
                    .AddTransient<DateTimeProvider>()
                    ;
            });
    }
}
