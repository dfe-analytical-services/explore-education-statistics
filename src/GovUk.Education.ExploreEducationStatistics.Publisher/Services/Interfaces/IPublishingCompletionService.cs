using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublishingCompletionService
{
    Task CompletePublishing(
        IReadOnlyList<ReleasePublishingKey> releaseVersionAndReleaseStatusIds,
        CancellationToken cancellationToken = default
    );
}
