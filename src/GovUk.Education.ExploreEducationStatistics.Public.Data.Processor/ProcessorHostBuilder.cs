using Azure.Core;
using Azure.Identity;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor;

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
            .ConfigureServices((hostBuilderContext, services) =>
            {
                var configuration = hostBuilderContext.Configuration;
                var hostEnvironment = hostBuilderContext.HostingEnvironment;

                // Only set up the `PublicDataDbContext` in non-integration test
                // environments. Otherwise, the connection string will be null and
                // cause the data source builder to throw a host exception.
                if (!hostEnvironment.IsIntegrationTest())
                {
                    var connectionString = configuration.GetConnectionString("PublicDataDb")!;

                    if (hostEnvironment.IsDevelopment())
                    {
                        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

                        // Set up the data source outside the `AddDbContext` action as this
                        // prevents `ManyServiceProvidersCreatedWarning` warnings due to EF
                        // creating over 20 `IServiceProvider` instances.
                        var dbDataSource = dataSourceBuilder.Build();

                        services.AddDbContext<PublicDataDbContext>(options =>
                        {
                            options
                                .UseNpgsql(dbDataSource)
                                .EnableSensitiveDataLogging();
                        });
                    }
                    else
                    {
                        services.AddDbContext<PublicDataDbContext>(options =>
                        {
                            var sqlServerTokenProvider = new DefaultAzureCredential();
                            var accessToken = sqlServerTokenProvider.GetToken(
                                new TokenRequestContext(scopes:
                                [
                                    "https://ossrdbms-aad.database.windows.net/.default"
                                ])).Token;

                            var connectionStringWithAccessToken =
                                connectionString.Replace("[access_token]", accessToken);

                            var dbDataSource = new NpgsqlDataSourceBuilder(connectionStringWithAccessToken).Build();

                            options.UseNpgsql(dbDataSource);
                        });
                    }
                }

                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights();
            });
    }
}
