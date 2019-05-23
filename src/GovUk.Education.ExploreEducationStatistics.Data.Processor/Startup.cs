using System;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(GovUk.Education.ExploreEducationStatistics.Data.Processor.Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("StatisticsDb");

            builder.Services
                .AddMemoryCache()
                .AddDbContext<ApplicationDbContext>(options =>
                    options
                        .UseSqlServer(connectionString))

                .AddTransient<IBlobService, BlobService>()
                .AddTransient<ISeedService, SeedService>()
                .AddTransient<IProcessorService, ProcessorService>()
                .AddTransient<IImporterService, ImporterService>()
                .AddTransient<ImporterFilterService>()
                .AddTransient<ImporterLocationService>()
                .AddTransient<ImporterMetaService>();
        }
    }
}
