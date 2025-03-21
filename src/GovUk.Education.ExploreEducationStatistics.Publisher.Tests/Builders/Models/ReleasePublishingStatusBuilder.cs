using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Models;

public class ReleasePublishingStatusBuilder(ReleasePublishingKey releasePublishingKey)
{
    private ReleasePublishingStatusContentStage _contentStatus = ReleasePublishingStatusContentStage.Complete;
    private ReleasePublishingStatusFilesStage _filesStatus = ReleasePublishingStatusFilesStage.Complete;

    public ReleasePublishingStatus Build()
    {
        var releasePublishingStatus = new ReleasePublishingStatus
        {
            
            PartitionKey = releasePublishingKey.ReleaseVersionId.ToString(),
            RowKey = releasePublishingKey.ReleaseStatusId.ToString(),
            State = new(
                _contentStatus,
                _filesStatus,
                ReleasePublishingStatusPublishingStage.NotStarted,
                ReleasePublishingStatusOverallStage.Started)
        };
        return releasePublishingStatus;
    }

    public ReleasePublishingStatusBuilder WhereContentStatusIs(ReleasePublishingStatusContentStage status)
    {
        _contentStatus = status;
        return this;
    }

    public ReleasePublishingStatusBuilder WhereFilesStatusIs(ReleasePublishingStatusFilesStage status)
    {
        _filesStatus = status;
        return this;
    }
}
