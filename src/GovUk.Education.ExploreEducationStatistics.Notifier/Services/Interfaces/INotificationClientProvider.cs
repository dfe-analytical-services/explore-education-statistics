using Notify.Client;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services.Interfaces;

public interface INotificationClientProvider
{
    NotificationClient Get();
}
