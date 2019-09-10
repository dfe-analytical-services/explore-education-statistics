using GovUk.Education.ExploreEducationStatistics.Notifier;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddTransient<IEmailService, EmailService>()
                .AddTransient<IStorageTableService, StorageTableService>()
                .AddTransient<ITokenService, TokenService>();
        }
    }
}