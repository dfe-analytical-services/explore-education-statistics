#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Utils
{
    public static class ConfigKeys
    {
        public const string StorageConnectionName = "TableStorageConnString";
        public const string NotifyApiKeyName = "NotifyApiKey";

        public const string SubscriptionsTblName = "Subscriptions";
        public const string BaseUrlName = "BaseUrl";
        public const string WebApplicationBaseUrlName = "WebApplicationBaseUrl";
        public const string TokenSecretKeyName = "TokenSecretKey";
        public const string PendingSubscriptionsTblName = "PendingSubscriptions";

        public const string ConfirmationEmailTemplateIdName = "SubscriptionConfirmationEmailTemplateId";
        public const string VerificationEmailTemplateIdName = "VerificationEmailTemplateId";
        public const string ReleaseEmailTemplateIdName =
            "ReleaseEmailTemplateId";
        public const string ReleaseAmendmentEmailTemplateIdName =
            "ReleaseAmendmentEmailTemplateId";
    }
}
