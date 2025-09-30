#nullable enable

using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class ReleasePublishingStatusRepository(
    IPublisherTableStorageService publisherTableStorageService
) : IReleasePublishingStatusRepository
{
    public async Task<IReadOnlyList<ReleasePublishingStatus>> GetAllByOverallStage(
        Guid releaseVersionId,
        params ReleasePublishingStatusOverallStage[] overallStages
    )
    {
        if (overallStages.Length == 0)
        {
            throw new ArgumentException("overallStages should not be empty");
        }

        var result = await publisherTableStorageService.QueryEntities<ReleasePublishingStatus>(
            PublisherReleaseStatusTableName,
            status => status.PartitionKey == releaseVersionId.ToString()
        );
        var allStatusesForReleaseVersion = await result.ToListAsync();

        var statusesForStages = allStatusesForReleaseVersion
            .Where(status =>
                overallStages.Contains(
                    Enum.Parse<ReleasePublishingStatusOverallStage>(status.OverallStage)
                )
            )
            .ToList();

        return statusesForStages;
    }

    public async Task RemovePublisherReleaseStatuses(IReadOnlyList<Guid> releaseVersionIds)
    {
        if (releaseVersionIds.IsNullOrEmpty())
        {
            return;
        }

        var filter = "";
        foreach (var releaseVersionId in releaseVersionIds)
        {
            var filterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                status.PartitionKey == releaseVersionId.ToString()
            );

            filter = filter == "" ? filterCondition : $"({filter}) or ({filterCondition})";
        }

        var results = await publisherTableStorageService.QueryEntities<ReleasePublishingStatus>(
            PublisherReleaseStatusTableName,
            filter
        );
        var statusesToRemove = await results.ToListAsync();

        await publisherTableStorageService.BatchManipulateEntities(
            PublisherReleaseStatusTableName,
            statusesToRemove,
            TableTransactionActionType.Delete
        );
    }
}
