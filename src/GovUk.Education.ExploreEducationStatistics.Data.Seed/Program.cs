using System.IO;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Seed.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Seed
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting application");

            var seedService = serviceProvider.GetService<ISeedService>();
            seedService.Seed();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            services.AddMemoryCache();

            services.AddLogging(builder => builder.AddConsole().AddConfiguration(configuration.GetSection("Logging")))
                .AddDbContext<ApplicationDbContext>(options =>
                    options
                        .UseSqlServer("Server=db;Database=master;User=SA;Password=Your_Password123;")
                        .EnableSensitiveDataLogging()
                )
                .AddTransient<ImporterFilterService>()
                .AddTransient<ImporterLocationService>()
                .AddTransient<ImporterMetaService>()
                .AddTransient<IImporterService, ImporterService>()
                .AddTransient<ISeedService, SeedService>()
                .BuildServiceProvider();
        }
    }
}