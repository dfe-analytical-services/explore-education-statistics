using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUK.Education.ExploreEducationStatistics.Data.Processor;
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
            var services = new ServiceCollection();

            services.AddMemoryCache();

            services.AddDbContext<ApplicationDbContext>(options =>
                options
                    .UseSqlServer("Server=db;Database=master;User=SA;Password=Your_Password123;")
                    .EnableSensitiveDataLogging()
            );

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
