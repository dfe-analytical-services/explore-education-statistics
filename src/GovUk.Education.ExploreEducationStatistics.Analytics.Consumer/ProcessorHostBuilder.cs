using GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Analytics.Requests.Consumer;

public static class ProcessorHostBuilder
{
    public static IHostBuilder ConfigureProcessorHostBuilder(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureAppConfiguration(builder =>
            {
                builder
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables();
            })
            .ConfigureLogging(logging =>
            {
                // TODO EES-5013 Why can't Command logging be suppressed via the logging config in application settings?
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            })
            .ConfigureServices((_, services) =>
            {
                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    .AddScoped<IAnalyticsPathResolver, IAnalyticsPathResolver>();
            });
    }
}
