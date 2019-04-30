using System;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUK.Education.ExploreEducationStatistics.Notifier;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]
namespace GovUK.Education.ExploreEducationStatistics.Notifier
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

            return services

            .AddTransient<IEmailService, EmailService>()
            .AddTransient<IStorageTableService, StorageTableService>()
            .AddTransient<ITokenService, TokenService>()

            // Important: We need to call CreateFunctionUserCategory, otherwise our log entries might be filtered out.
            .AddSingleton<ILogger>(_ => _loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Common")))
            .BuildServiceProvider();
        }
    }
}
