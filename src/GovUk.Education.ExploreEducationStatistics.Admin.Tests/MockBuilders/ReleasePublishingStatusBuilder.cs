using System;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class ReleasePublishingStatusBuilder
{
    public ReleasePublishingStatus Build()
    {
        return new ReleasePublishingStatus(
            releaseVersionId: Guid.NewGuid(),
            releaseStatusId: Guid.NewGuid(),
            publicationSlug: "publication-slug",
            publish: null,
            releaseSlug: "release-slug",
            state: ReleasePublishingStatusStates.ImmediateReleaseStartedState,
            immediate: true);
    }
}