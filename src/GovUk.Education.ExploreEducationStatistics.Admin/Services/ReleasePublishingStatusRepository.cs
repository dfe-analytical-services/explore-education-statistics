using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Cosmos.Table;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleasePublishingStatusRepository : IReleasePublishingStatusRepository
    {
        private readonly IPublisherTableStorageService _publisherTableStorageService;

        public ReleasePublishingStatusRepository(IPublisherTableStorageService publisherTableStorageService)
        {
            _publisherTableStorageService = publisherTableStorageService;
        }

        public Task<IReadOnlyList<ReleasePublishingStatus>> GetAllByOverallStage(
            Guid releaseVersionId,
            params ReleasePublishingStatusOverallStage[] overallStages)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatus.PartitionKey),
                QueryComparisons.Equal,
                releaseVersionId.ToString());

            if (overallStages.Any())
            {
                var allStageFilters = overallStages.ToList().Aggregate("", (acc, stage) =>
                {
                    var stageFilter = TableQuery.GenerateFilterCondition(
                        nameof(ReleasePublishingStatus.OverallStage),
                        QueryComparisons.Equal,
                        stage.ToString()
                    );

                    if (acc == "")
                    {
                        return stageFilter;
                    }

                    return TableQuery.CombineFilters(acc, TableOperators.Or, stageFilter);
                });

                filter = TableQuery.CombineFilters(filter, TableOperators.And, allStageFilters);
            }

            var query = new TableQuery<ReleasePublishingStatus>().Where(filter);
            return _publisherTableStorageService.ExecuteQuery(PublisherReleaseStatusTableName, query);
        }

        public async Task RemovePublisherReleaseStatuses(IReadOnlyList<Guid> releaseVersionIds)
        {
            if (releaseVersionIds.IsNullOrEmpty())
            {
                // Return early as we want to do nothing in this case - without this,
                // `filter` will be string.Empty and the query returns all table entities
                return;
            }

            var filter = string.Empty;
            foreach (var releaseVersionId in releaseVersionIds)
            {
                var newFilter = TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatus.PartitionKey),
                    QueryComparisons.Equal, releaseVersionId.ToString());

                filter = filter == string.Empty
                    ? newFilter
                    : TableQuery.CombineFilters(filter, TableOperators.Or, newFilter);
            }

            var cloudTable = _publisherTableStorageService.GetTable(PublisherReleaseStatusTableName);
            var query = new TableQuery<ReleasePublishingStatus>().Where(filter);
            var releaseStatusesToRemove = cloudTable.ExecuteQuery(query);

            foreach (var releaseStatus in releaseStatusesToRemove)
            {
                await cloudTable.ExecuteAsync(TableOperation.Delete(releaseStatus));
            }
        }
    }
}
