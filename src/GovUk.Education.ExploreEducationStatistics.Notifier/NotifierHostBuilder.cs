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
using Microsoft.Extensions.Options;
using Notify.Client;
using Notify.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Notifier;

public static class NotifierHostBuilder
{
    public static IHostBuilder ConfigureNotifierHostBuilder(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureAppConfiguration(builder =>
            {
                builder
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
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
                services
                    .AddApplicationInsightsTelemetryWorkerService()
                    .ConfigureFunctionsApplicationInsights()
                    // @MarkFix Create migration with CREATE USER and GRANTs for access required
                    // @MarkFix add sqlNotifierUser to release infra deploy validate/deploy steps
                    // @MarkFix add sqlNotifierUserPassword to release infra deploy validate/deploy steps
                    .AddDbContext<ContentDbContext>(options =>
                        options.UseSqlServer(
                            ConnectionUtils.GetAzureSqlConnectionString("ContentDb"),
                            providerOptions =>
                                SqlServerDbContextOptionsBuilderExtensions.EnableCustomRetryOnFailure(providerOptions)))
                    .AddFluentValidation()
                    .AddValidatorsFromAssembly(
                        typeof(ApiNotificationMessage.Validator).Assembly) // Adds *all* validators from Notifier.Model
                    .Configure<AppOptions>(hostContext.Configuration.GetSection(AppOptions.Section))
                    .Configure<GovUkNotifyOptions>(hostContext.Configuration.GetSection(GovUkNotifyOptions.Section))
                    .AddTransient<INotificationClient>(serviceProvider =>
                    {
                        var govUkNotifyOptions = serviceProvider.GetRequiredService<IOptions<GovUkNotifyOptions>>();

                        return new NotificationClient(govUkNotifyOptions.Value.ApiKey);
                    })
                    .AddTransient<INotifierTableStorageService, NotifierTableStorageService>()
                    .AddTransient<IEmailService, EmailService>()
                    .AddTransient<IApiSubscriptionRepository, ApiSubscriptionRepository>()
                    .AddTransient<ISubscriptionRepository, SubscriptionRepository>()
                    .AddTransient<IApiSubscriptionService, ApiSubscriptionService>()
                    .AddTransient<ITokenService, TokenService>();
            });
    }
}
