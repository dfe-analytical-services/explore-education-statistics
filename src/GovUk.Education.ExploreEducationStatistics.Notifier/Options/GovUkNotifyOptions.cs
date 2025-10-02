using System;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Notifier.Validators;

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

        /// <summary>
        /// The email template to use when a new non-breaking dataset version is published (e.g., 1.0, 2.1, 3.1 etc)
        /// </summary>
        public string ApiSubscriptionDataSetVersionPublishedId { get; init; } = null!;

        /// <summary>
        /// The email template to use when a new dataset version with breaking changes is published (e.g., 2.0, 3.0, 4.0, 5.0 etc)
        /// </summary>
        public string ApiSubscriptionMajorDataSetVersionPublishedId { get; init; } = null!;

        public string ApiSubscriptionVerificationId { get; init; } = null!;

        public string ReleasePublishingFeedbackId { get; init; } = null!;

        /// <summary>
        /// Returns the email template to use when a new breaking or non-breaking dataset version is published
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        public string SelectDataSetPublishedTemplateId(string version)
        {
            if (!DataSetVersionNumber.TryParse(version, out var dataSetVersionNumber))
            {
                throw new ArgumentException(ValidationMessages.InvalidDataSetVersion.Message);
            }
            var isNewMajorVersion =
                dataSetVersionNumber.Major >= 2 && dataSetVersionNumber.Patch == 0 && dataSetVersionNumber.Minor == 0;
            return isNewMajorVersion
                ? ApiSubscriptionMajorDataSetVersionPublishedId
                : ApiSubscriptionDataSetVersionPublishedId;
        }
    }
}
