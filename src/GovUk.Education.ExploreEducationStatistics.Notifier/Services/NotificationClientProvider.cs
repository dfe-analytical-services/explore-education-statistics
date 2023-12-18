using Notify.Client;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Services
{
    public class NotificationClientProvider : INotificationClientProvider
    {
        public NotificationClient Get(string notifyApiKey)
        {
            return new NotificationClient(notifyApiKey);
        }
    }
}
