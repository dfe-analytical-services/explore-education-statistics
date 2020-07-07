using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseStatusRepository : IReleaseStatusRepository
    {
        private const string TableName = "ReleaseStatus";

        private readonly ITableStorageService _publisherTableStorageService;

        public ReleaseStatusRepository(ITableStorageService publisherTableStorageService)
        {
            _publisherTableStorageService = publisherTableStorageService;
        }

        public Task<IEnumerable<ReleaseStatus>> GetAllByOverallStage(Guid releaseId, params ReleaseStatusOverallStage[] overallStages)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PartitionKey),
                    QueryComparisons.Equal, releaseId.ToString());

            if (overallStages.Any())
            {
                var allStageFilters = overallStages.ToList().Aggregate("", (acc, stage) => 
                {
                   var stageFilter = TableQuery.GenerateFilterCondition(
                       nameof(ReleaseStatus.OverallStage),
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

            var query = new TableQuery<ReleaseStatus>().Where(filter);
            return _publisherTableStorageService.ExecuteQueryAsync(TableName, query);
        }
    }
}