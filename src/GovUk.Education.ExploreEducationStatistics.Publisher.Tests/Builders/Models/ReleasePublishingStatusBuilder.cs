using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;

public class ReleasePublishingStatusBuilder(ReleasePublishingKey releasePublishingKey)
{
    private ReleasePublishingStatusFilesStage _filesStatus = ReleasePublishingStatusFilesStage.Complete;

    public ReleasePublishingStatus Build()
    {
        var releasePublishingStatus = new ReleasePublishingStatus
        {
            PartitionKey = releasePublishingKey.ReleaseVersionId.ToString(),
            RowKey = releasePublishingKey.ReleaseStatusId.ToString(),
            State = new ReleasePublishingStatusState(
                _filesStatus,
                ReleasePublishingStatusPublishingStage.NotStarted,
                ReleasePublishingStatusOverallStage.Started
            ),
        };
        return releasePublishingStatus;
    }

    public ReleasePublishingStatusBuilder WhereFilesStatusIs(ReleasePublishingStatusFilesStage status)
    {
        _filesStatus = status;
        return this;
    }
}
