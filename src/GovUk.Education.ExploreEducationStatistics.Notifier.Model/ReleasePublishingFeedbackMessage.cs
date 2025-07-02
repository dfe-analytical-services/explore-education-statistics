using System;

namespace GovUk.Education.ExploreEducationStatistics.Notifier.Model;

public record ReleasePublishingFeedbackMessage(Guid ReleasePublishingFeedbackId, string EmailAddress);
