namespace GovUk.Education.ExploreEducationStatistics.Notifier.Configuration;

public class GovUkNotifyOptions
{
    public const string GovUkNotify = "GovUkNotify";

    public string ApiKey { get; init; } = null!;

    public EmailTemplateOptions EmailTemplates { get; init; } = null!;

    public class EmailTemplateOptions
    {
        public string ReleaseAmendmentPublishedId { get; init; } = null!;

        public string ReleaseAmendmentPublishedSupersededSubscribersId { get; init; } = null!;

        public string ReleasePublishedId { get; init; } = null!;

        public string ReleasePublishedSupersededSubscribersId { get; init; } = null!;

        public string SubscriptionConfirmationId { get; init; } = null!;

        public string SubscriptionVerificationId { get; init; } = null!;
    }
}