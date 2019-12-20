using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseStatusService : IReleaseStatusService
    {
        private readonly IMapper _mapper;
        private readonly ITableStorageService _tableStorageService;
        private const string TableName = "ReleaseStatus";

        public ReleaseStatusService(IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _tableStorageService = new TableStorageService(config.GetConnectionString("PublisherStorage"));
        }

        public async Task<IEnumerable<ReleaseStatusViewModel>> GetReleaseStatusesAsync(Guid releaseId)
        {
            var query = new TableQuery<ReleaseStatus>().Where(
                TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PartitionKey), QueryComparisons.Equal,
                    releaseId.ToString()));

            var results = new List<ReleaseStatus>();
            var table = await GetTableAsync();
            TableContinuationToken token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                results.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return _mapper.Map<IEnumerable<ReleaseStatusViewModel>>(results);
        }

        private async Task<CloudTable> GetTableAsync()
        {
            return await _tableStorageService.GetTableAsync(TableName);
        }
    }
}