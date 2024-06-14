using Dapper;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
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
                    var connectionString = ConnectionUtils.GetPostgreSqlConnectionString("PublicDataDb")!;
                    services.AddFunctionAppPsqlDbContext<PublicDataDbContext>(connectionString, hostBuilderContext);
                }

                // Configure Dapper to match CSV columns with underscores
                DefaultTypeMap.MatchNamesWithUnderscores = true;

                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    .AddDbContext<ContentDbContext>(options =>
                        options
                            .UseSqlServer(configuration.GetConnectionString("ContentDb"),
                                providerOptions => providerOptions.EnableCustomRetryOnFailure())
                            .EnableSensitiveDataLogging(hostEnvironment.IsDevelopment()))
                    .AddFluentValidation()
                    .AddScoped<IDataSetVersionPathResolver, DataSetVersionPathResolver>()
                    .AddScoped<IDataSetService, DataSetService>()
                    .AddScoped<IDataSetVersionService, DataSetVersionService>()
                    .AddScoped<IDataSetMetaService, DataSetMetaService>()
                    .AddScoped<IDataDuckDbRepository, DataDuckDbRepository>()
                    .AddScoped<IFilterOptionsDuckDbRepository, FilterOptionsDuckDbRepository>()
                    .AddScoped<IIndicatorsDuckDbRepository, IndicatorsDuckDbRepository>()
                    .AddScoped<ILocationsDuckDbRepository, LocationsDuckDbRepository>()
                    .AddScoped<ITimePeriodsDuckDbRepository, TimePeriodsDuckDbRepository>()
                    .AddScoped<IFilterMetaRepository, FilterMetaRepository>()
                    .AddScoped<IGeographicLevelMetaRepository, GeographicLevelMetaRepository>()
                    .AddScoped<IIndicatorMetaRepository, IndicatorMetaRepository>()
                    .AddScoped<ILocationMetaRepository, LocationMetaRepository>()
                    .AddScoped<ITimePeriodMetaRepository, TimePeriodMetaRepository>()
                    .AddScoped<IParquetService, ParquetService>()
                    .AddScoped<IPrivateBlobStorageService, PrivateBlobStorageService>()
                    .AddScoped<IValidator<DataSetCreateRequest>,
                        DataSetCreateRequest.Validator>()
                    .Configure<DataFilesOptions>(
                        hostBuilderContext.Configuration.GetSection(DataFilesOptions.Section));
            });
    }
}
