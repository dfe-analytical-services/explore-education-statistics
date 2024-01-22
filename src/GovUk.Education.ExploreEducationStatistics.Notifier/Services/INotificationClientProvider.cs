using Notify.Client;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public interface INotificationClientProvider
    {
        NotificationClient Get(string notifyApiKey);
    }
}
