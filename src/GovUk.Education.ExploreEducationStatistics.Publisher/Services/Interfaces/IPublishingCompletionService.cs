using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublishingCompletionService
{
    Task CompletePublishingIfAllPriorStagesComplete(
        IReadOnlyList<ReleasePublishingKey> releaseVersionAndReleaseStatusIds);
}
