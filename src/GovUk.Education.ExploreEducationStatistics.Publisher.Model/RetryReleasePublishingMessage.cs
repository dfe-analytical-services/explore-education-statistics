#nullable enable

using System;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public record RetryReleasePublishingMessage(Guid ReleaseId);