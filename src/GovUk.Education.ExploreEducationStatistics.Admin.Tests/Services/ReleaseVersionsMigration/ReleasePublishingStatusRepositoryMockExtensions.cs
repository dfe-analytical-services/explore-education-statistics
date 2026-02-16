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
    public static IReturnsResult<IReleasePublishingStatusRepository> SetupGetAllByOverallStageCompleteReturnsNoResults(
        this Mock<IReleasePublishingStatusRepository> mock,
        Guid releaseVersionId
    ) =>
        mock.Setup(m => m.GetAllByOverallStage(releaseVersionId, ReleasePublishingStatusOverallStage.Complete))
            .ReturnsAsync([]);

    public static IReturnsResult<IReleasePublishingStatusRepository> SetupGetAllByOverallStageCompleteReturnsSingleResult(
        this Mock<IReleasePublishingStatusRepository> mock,
        Guid releaseVersionId,
        DateTimeOffset timestamp,
        bool immediate = false,
        DateTime? created = null
    ) =>
        mock.Setup(m => m.GetAllByOverallStage(releaseVersionId, ReleasePublishingStatusOverallStage.Complete))
            .ReturnsAsync([
                new ReleasePublishingStatus
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = releaseVersionId.ToString(),
                    Immediate = immediate,
                    Created = created ?? DateTime.UtcNow,
                    Timestamp = timestamp,
                },
            ]);
}
