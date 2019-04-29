using System;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUK.Education.ExploreEducationStatistics.Data.Processor;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUK.Education.ExploreEducationStatistics.Data.Processor.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]
namespace GovUK.Education.ExploreEducationStatistics.Data.Processor
{
    internal class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) =>
            builder.AddDependencyInjection<ServiceProviderBuilder>();
    }

    internal class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private readonly ILoggerFactory _loggerFactory;

        public ServiceProviderBuilder(ILoggerFactory loggerFactory) =>
            _loggerFactory = loggerFactory;

        public IServiceProvider Build()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("SQLConnectionString");
            var services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseSqlServer(connectionString)
                    .EnableSensitiveDataLogging());

            services.AddTransient<IBlobService, BlobService>();
            services.AddTransient<ISeedService, SeedService>();
            services.AddTransient<IImporterService, ImporterService>();
            services.AddTransient<ImporterFilterService>();
            services.AddTransient<ImporterLocationService>();
            services.AddTransient<ImporterMetaService>();

            // Important: We need to call CreateFunctionUserCategory, otherwise our log entries might be filtered out.
            services.AddSingleton<ILogger>(_ => _loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Common")));

            return services.BuildServiceProvider();
        }
    }
}
