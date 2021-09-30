using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Cosmos.Table;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleasePublishingStatusRepository : IReleasePublishingStatusRepository
    {
        private readonly ITableStorageService _publisherTableStorageService;

        public ReleasePublishingStatusRepository(ITableStorageService publisherTableStorageService)
        {
            _publisherTableStorageService = publisherTableStorageService;
        }

        public Task<IEnumerable<ReleasePublishingStatus>> GetAllByOverallStage(Guid releaseId, params ReleasePublishingStatusOverallStage[] overallStages)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatus.PartitionKey),
                    QueryComparisons.Equal, releaseId.ToString());

            if (overallStages.Any())
            {
                var allStageFilters = overallStages.ToList().Aggregate("", (acc, stage) => 
                {
                   var stageFilter = TableQuery.GenerateFilterCondition(
                       nameof(ReleasePublishingStatus.OverallStage),
                       QueryComparisons.Equal,
                       stage.ToString()
                    );
                
                    if (acc == "")  {
                        return stageFilter;
                    }

                    return TableQuery.CombineFilters(acc, TableOperators.Or, stageFilter); 
                });

                filter = TableQuery.CombineFilters(filter, TableOperators.And, allStageFilters);
            }

            var query = new TableQuery<ReleasePublishingStatus>().Where(filter);
            return _publisherTableStorageService.ExecuteQueryAsync(PublisherReleaseStatusTableName, query);
        }
    }
}