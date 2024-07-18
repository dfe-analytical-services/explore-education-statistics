namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public static class Constants
{
    public static class NotifierTableStorageTableNames
    {
        public const string PublicationPendingSubscriptionsTableName = "PendingSubscriptions";
        public const string PublicationSubscriptionsTableName = "Subscriptions";
        public const string ApiSubscriptionsTableName = "ApiSubscriptions";
    }

    public static class  NotifierQueueStorage
    {
        public const string ReleaseNotificationQueue = "release-notifications";
        public const string ApiNotificationQueue = "api-notifications";
    }
}
