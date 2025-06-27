namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public static class NotifierTableStorage
{
    public const string PublicationPendingSubscriptionsTable = "PendingSubscriptions";
    public const string PublicationSubscriptionsTable = "Subscriptions";
    public const string ApiSubscriptionsTable = "ApiSubscriptions";
}

public static class NotifierQueueStorage
{
    public const string ReleaseNotificationQueue = "release-notifications";
    public const string ApiNotificationQueue = "api-notifications";
    public const string ReleasePublishingFeedbackQueue = "release-publishing-feedback";
}

public static class NotifierEmailTemplateFields
{
    public const string DataSetTitle = "data_set_title";
    public const string DataSetUrl = "data_set_url";
    public const string DataSetVersion = "data_set_version";
    public const string UnsubscribeUrl = "unsubscribe_url";
    public const string VerificationUrl = "verification_url";
}
