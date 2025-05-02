using Dapper;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Options;
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
using Microsoft.Extensions.Options;

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
                var isIntegrationTest = hostEnvironment.IsIntegrationTest();
                if (!isIntegrationTest)
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
                    .AddScoped<IReleaseFileRepository, ReleaseFileRepository>()
                    .AddScoped<IDataSetVersionService, DataSetVersionService>()
                    .AddScoped<IDataSetMetaService, DataSetMetaService>()
                    .AddScoped<IDataSetVersionMappingService, DataSetVersionMappingService>()
                    .AddScoped<IDataSetVersionChangeService, DataSetVersionChangeService>()
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
                    .AddScoped<IPrivateBlobStorageService, PrivateBlobStorageService>(provider =>
                        new PrivateBlobStorageService(
                            provider.GetRequiredService<IOptions<AppOptions>>().Value
                                .PrivateStorageConnectionString,
                            provider.GetRequiredService<ILogger<IBlobStorageService>>()))
                    .Configure<AppOptions>(
                        hostBuilderContext.Configuration.GetSection(AppOptions.Section))
                    .Configure<DataFilesOptions>(
                        hostBuilderContext.Configuration.GetSection(DataFilesOptions.Section));
                
                var section = configuration.GetSection("FeatureFlags");
                if (!section.Exists())
                {
                    Console.WriteLine("Warning: FeatureFlags section is missing from configuration. Using defaults.");
                    if (isIntegrationTest)
                    {
                        //Set this feature flag to on so that the tests related to new functionlity EES-5779 always run
                        services.Configure<FeatureFlags>(options =>
                        {
                            options.EnableReplacementOfPublicApiDataSets = true;
                        });
                    }
                    else
                    {
                        services.Configure<FeatureFlags>(options =>
                        {
                            options.EnableReplacementOfPublicApiDataSets = false;
                        });
                    }
                }
                else
                {
                    services.Configure<FeatureFlags>(section);
                }
                
                services.AddValidatorsFromAssembly(typeof(DataSetCreateRequest.Validator).Assembly); // Adds *all* validators from Public.Data.Processor
            });
    }
}
