using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
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
                .AddAutoMapper()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(GetConnectionString("StatisticsDb", $"{ConnectionTypeValues[ConnectionTypes.AZURE_SQL]}"),
                        providerOptions => providerOptions.EnableRetryOnFailure()))
                .AddTransient<IFileStorageService, FileStorageService>(s => new FileStorageService(GetConnectionString("CoreStorage", $"{ConnectionTypeValues[ConnectionTypes.AZURE_STORAGE]}")))
                .AddTransient<IFileImportService, FileImportService>()
                .AddTransient<ImporterSchoolService>()
                .AddTransient<IImporterService, ImporterService>()
                .AddTransient<ISplitFileService, SplitFileService>()
                .AddTransient<ImporterFilterService>()
                .AddTransient<ImporterLocationService>()
                .AddTransient<IImporterMetaService, ImporterMetaService>()
                .AddTransient<IReleaseProcessorService, ReleaseProcessorService>()
                .AddTransient<ImporterMemoryCache>()
                .AddTransient<ITableStorageService, TableStorageService>(s => new TableStorageService(GetConnectionString("CoreStorage", $"{ConnectionTypeValues[ConnectionTypes.AZURE_STORAGE]}")))
                .AddTransient<IBatchService, BatchService>()
                .AddSingleton<IValidatorService, ValidatorService>()
                .BuildServiceProvider();
        }

        private static string GetConnectionString(string name, string connectionTypeValue)
        {
            // Attempt to get a connection string defined for running locally.
            // Settings in the local.settings.json file are only used by Functions tools when running locally.
            var connectionString =
                Environment.GetEnvironmentVariable($"ConnectionStrings:{name}", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(connectionString))
            {
                // Get the connection string from the Azure Functions App using the naming convention for type SQLAzure.
                connectionString = Environment.GetEnvironmentVariable($"{connectionTypeValue}_{name}", EnvironmentVariableTarget.Process);
            }

            return connectionString;
        }
        
        private enum ConnectionTypes
        {
            AZURE_STORAGE,
            AZURE_SQL
        }
        
        private static readonly Dictionary<ConnectionTypes, string> ConnectionTypeValues =
            new Dictionary<ConnectionTypes, string>
            {
                {
                    ConnectionTypes.AZURE_STORAGE, "CUSTOMCONNSTR"
                },
                {
                    ConnectionTypes.AZURE_SQL, "SQLAZURECONNSTR"
                }
            };
    }
}