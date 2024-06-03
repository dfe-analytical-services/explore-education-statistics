using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Options;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor;

public static class ProcessorHostBuilder
{
    public static IHostBuilder ConfigureProcessorHostBuilder(this IHostBuilder hostBuilder) =>
        hostBuilder
            .ConfigureAppConfiguration(builder =>
            {
                builder
                    .AddJsonFile("appsettings.json", true, false)
                    .AddJsonFile("appsettings.Local.json", true, false)
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
                    var connectionString = ConnectionUtils.GetPostgreSqlConnectionString("PublicDataDb")!;
                    services.AddFunctionAppPsqlDbContext<PublicDataDbContext>(connectionString, hostBuilderContext);
                }

                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    .AddDbContext<ContentDbContext>(options =>
                        options
                            .UseSqlServer(configuration.GetConnectionString("ContentDb"),
                                providerOptions => providerOptions.EnableCustomRetryOnFailure())
                            .EnableSensitiveDataLogging(hostEnvironment.IsDevelopment()))
                    .AddFluentValidation()
                    .AddScoped<IDataSetService, DataSetService>()
                    .AddScoped<IDataSetVersionPathResolver, DataSetVersionPathResolver>()
                    .AddScoped<IPrivateBlobStorageService, PrivateBlobStorageService>()
                    .AddScoped<IValidator<InitialDataSetVersionCreateRequest>,
                        InitialDataSetVersionCreateRequest.Validator>()
                    .Configure<ParquetFilesOptions>(
                        hostBuilderContext.Configuration.GetSection(ParquetFilesOptions.Section));
            });
}
