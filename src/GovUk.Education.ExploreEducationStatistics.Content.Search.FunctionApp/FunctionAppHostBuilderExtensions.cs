using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp;

public static class PublisherHostBuilderExtensions
{
    public static IHostBuilder ConfigureHostBuilder(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureAppConfiguration((hostBuilderContext, configBuilder) =>
            {
                configBuilder
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .AddConfiguration(hostBuilderContext.Configuration);
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
                    .ConfigureFunctionsApplicationInsights();
            });
    }
}
