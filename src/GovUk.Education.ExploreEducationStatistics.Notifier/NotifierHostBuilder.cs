using System.IO;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Functions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Notifier.Model;
using GovUk.Education.ExploreEducationStatistics.Notifier.Options;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories;
using GovUk.Education.ExploreEducationStatistics.Notifier.Repositories.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notify.Client;
using Notify.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier;

public static class NotifierHostBuilder
{
    public static IHostBuilder ConfigureNotifierHostBuilder(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false)
                    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables();
            })
            .ConfigureLogging(logging =>
            {
                // TODO EES-5013 Why can't this be controlled through application settings?
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var appOptions = hostContext
                    .Configuration
                    .GetRequiredSection(AppOptions.Section);

                var govUkNotifyOptions = hostContext
                    .Configuration
                    .GetRequiredSection(GovUkNotifyOptions.Section);

                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    .AddDbContext<ContentDbContext>(options =>
                        options.UseSqlServer(
                            ConnectionUtils.GetAzureSqlConnectionString("ContentDb"),
                            providerOptions =>
                                SqlServerDbContextOptionsBuilderExtensions.EnableCustomRetryOnFailure(providerOptions)))
                    .AddFluentValidation()
                    .AddValidatorsFromAssembly(
                        typeof(ApiNotificationMessage.Validator).Assembly) // Adds *all* validators from Notifier.Model
                    .Configure<AppOptions>(appOptions)
                    .Configure<GovUkNotifyOptions>(govUkNotifyOptions)
                    .AddTransient<INotificationClient>(_ => new NotificationClient(
                        govUkNotifyOptions.GetValue<string>(nameof(GovUkNotifyOptions.ApiKey))))
                    .AddTransient<INotifierTableStorageService, NotifierTableStorageService>()
                    .AddTransient<IApiSubscriptionRepository, ApiSubscriptionRepository>()
                    .AddTransient<ISubscriptionRepository, SubscriptionRepository>()
                    .AddTransient<IApiSubscriptionService, ApiSubscriptionService>()
                    .AddTransient<ITokenService, TokenService>();

                if (appOptions.GetValue<bool>(nameof(AppOptions.EmailEnabled)))
                {
                    services.AddTransient<IEmailService, EmailService>();
                }
                else
                {
                    services.AddTransient<IEmailService, LoggingEmailService>();
                }
            });
    }
}
