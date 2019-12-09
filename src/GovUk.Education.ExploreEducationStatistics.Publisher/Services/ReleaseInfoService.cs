using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseInfoService : IReleaseInfoService
    {
        private readonly ITableStorageService _tableStorageService;

        private const string TableName = "releases";

        public ReleaseInfoService(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task AddReleaseInfoAsync(QueueReleaseMessage message, ReleaseInfoStatus status)
        {
            var releaseInfoTable = await GetReleaseInfoTableAsync();
            var releaseInfo = new ReleaseInfo(message.PublicationSlug,
                message.PublishScheduled,
                message.ReleaseId,
                message.ReleaseSlug,
                status);
            await releaseInfoTable.ExecuteAsync(TableOperation.InsertOrReplace(releaseInfo));
        }

        public async Task<IEnumerable<ReleaseInfo>> GetScheduledReleasesAsync()
        {
            var results = new List<ReleaseInfo>();

            var releaseInfoTable = await GetReleaseInfoTableAsync();
            TableContinuationToken token = null;
            do
            {
                // TODO EES-863 Currently returns all scheduled releases
                // TODO EES-863 Only query scheduled releases that are being published today
                var tableQuery = new TableQuery<ReleaseInfo>()
                    .Where(TableQuery.GenerateFilterCondition("Status",
                        QueryComparisons.Equal, ReleaseInfoStatus.Scheduled.ToString()));

                var queryResult = await releaseInfoTable.ExecuteQuerySegmentedAsync(tableQuery, token);

                results.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return results;
        }

        public async Task UpdateReleaseInfoStatusAsync(Guid releaseId, string rowKey, ReleaseInfoStatus status)
        {
            var releaseInfoTable = await GetReleaseInfoTableAsync();
            var tableResult = await releaseInfoTable.ExecuteAsync(
                TableOperation.Retrieve<ReleaseInfo>(releaseId.ToString(), rowKey));

            if (tableResult.Result is ReleaseInfo releaseInfo)
            {
                releaseInfo.Status = status.ToString();
                await releaseInfoTable.ExecuteAsync(TableOperation.InsertOrMerge(releaseInfo));
            }
        }

        private async Task<CloudTable> GetReleaseInfoTableAsync()
        {
            return await _tableStorageService.GetTableAsync(TableName);
        }
    }
}