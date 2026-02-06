using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Moq;
using Moq.Language.Flow;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
internal static class ReleasePublishingStatusRepositoryMockExtensions
{
    public static IReturnsResult<IReleasePublishingStatusRepository> SetupGetAllByOverallStageReturnsNoResults(
        this Mock<IReleasePublishingStatusRepository> mock,
        Guid releaseVersionId,
        ReleasePublishingStatusOverallStage overallStage
    ) => mock.Setup(m => m.GetAllByOverallStage(releaseVersionId, overallStage)).ReturnsAsync([]);

    public static IReturnsResult<IReleasePublishingStatusRepository> SetupGetAllByOverallStageReturnsSingleResult(
        this Mock<IReleasePublishingStatusRepository> mock,
        Guid releaseVersionId,
        ReleasePublishingStatusOverallStage overallStage,
        bool immediate = false,
        DateTime? created = null,
        DateTimeOffset? timestamp = null
    ) =>
        mock.Setup(m => m.GetAllByOverallStage(releaseVersionId, overallStage))
            .ReturnsAsync([
                new ReleasePublishingStatus
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = releaseVersionId.ToString(),
                    Immediate = immediate,
                    Created = created ?? DateTime.UtcNow,
                    Timestamp = timestamp ?? DateTimeOffset.UtcNow,
                },
            ]);
}
