namespace GovUk.Education.ExploreEducationStatistics.Notifier.Options;

public class GovUkNotifyOptions
{
    public const string Section = "GovUkNotify";

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

        public string ApiSubscriptionConfirmationId { get; init; } = null!;

        public string ApiSubscriptionMinorDataSetVersionId { get; init; } = null!;
        public string ApiSubscriptionMajorDataSetVersionId { get; init; } = null!;

        public string ApiSubscriptionVerificationId { get; init; } = null!;
    }
}
