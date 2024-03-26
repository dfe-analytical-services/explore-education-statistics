using GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;
using GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;
using Microsoft.Extensions.Options;
using Notify.Client;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services;

public class NotificationClientProvider(IOptions<GovUkNotifyOptions> govUkNotifyOptions)
    : INotificationClientProvider
{
    private readonly GovUkNotifyOptions _govUkNotifyOptions = govUkNotifyOptions.Value;

    public NotificationClient Get()
    {
        return new NotificationClient(_govUkNotifyOptions.ApiKey);
    }
}
