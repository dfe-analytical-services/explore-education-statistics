using System;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(GovUk.Education.ExploreEducationStatistics.Data.Processor.Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddMemoryCache()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(GetSqlAzureConnectionString("StatisticsDb")))
                .AddTransient<IFileStorageService, FileStorageService>()
                .AddTransient<IImportService, ImportService>()
                .AddTransient<IImporterService, ImporterService>()
                .AddTransient<ImporterFilterService>()
                .AddTransient<ImporterLocationService>()
                .AddTransient<ImporterMetaService>()
                .BuildServiceProvider();
        }

        private static string GetSqlAzureConnectionString(string name)
        {
            // Attempt to get a connection string defined for running locally.
            // Settings in the local.settings.json file are only used by Functions tools when running locally.
            var connectionString =
                Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(connectionString))
            {
                // Get the connection string from the Azure Functions App using the naming convention for type SQLAzure.
                connectionString = Environment.GetEnvironmentVariable(
                    $"SQLAZURECONNSTR_{name}",
                    EnvironmentVariableTarget.Process);
            }

            return connectionString;
        }
    }
}