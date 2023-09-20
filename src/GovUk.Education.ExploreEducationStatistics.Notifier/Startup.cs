#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Notifier;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Utils;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notify.Client;
using Notify.Interfaces;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GovUk.Education.ExploreEducationStatistics.Notifier
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddTransient<IEmailService, EmailService>()
                .AddTransient<ISubscriberService, SubscriberService>()
                .AddTransient<ITokenService, TokenService>()
                .AddTransient<INotificationClient, NotificationClient>(provider =>
                {
                    var apiKey = GetConfigurationValue(provider, ConfigKeys.NotifyApiKeyName);
                    return new NotificationClient(apiKey);
                });
        }

        private static string GetConfigurationValue(IServiceProvider provider, string key)
        {
            var configuration = provider.GetService<IConfiguration>();
            return configuration.GetValue<string>(key);
        }
    }
}
