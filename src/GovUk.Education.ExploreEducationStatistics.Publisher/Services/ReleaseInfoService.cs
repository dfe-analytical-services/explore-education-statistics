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

        public async Task UpdateContentStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoTaskStatus status)
        {
            await UpdateReleaseInfoAsync(releaseId, releaseInfoId, releaseInfo =>
            {
                releaseInfo.ReleaseContentStatus = status.ToString();
                return releaseInfo;
            });
        }

        public async Task UpdateDataStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoTaskStatus status)
        {
            await UpdateReleaseInfoAsync(releaseId, releaseInfoId, releaseInfo =>
            {
                releaseInfo.ReleaseDataStatus = status.ToString();
                return releaseInfo;
            });
        }

        public async Task UpdateFilesStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoTaskStatus status)
        {
            await UpdateReleaseInfoAsync(releaseId, releaseInfoId, releaseInfo =>
            {
                releaseInfo.ReleaseFilesStatus = status.ToString();
                return releaseInfo;
            });
        }

        public async Task UpdateReleaseInfoStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoStatus status)
        {
            await UpdateReleaseInfoAsync(releaseId, releaseInfoId, releaseInfo =>
            {
                releaseInfo.Status = status.ToString();
                return releaseInfo;
            });
        }

        private async Task UpdateReleaseInfoAsync(Guid releaseId, Guid releaseInfoId,
            Func<ReleaseInfo, ReleaseInfo> updateFunction)
        {
            var releaseInfoTable = await GetReleaseInfoTableAsync();
            var tableResult = await releaseInfoTable.ExecuteAsync(
                TableOperation.Retrieve<ReleaseInfo>(releaseId.ToString(), releaseInfoId.ToString()));

            if (tableResult.Result is ReleaseInfo releaseInfo)
            {
                await releaseInfoTable.ExecuteAsync(TableOperation.InsertOrMerge(updateFunction.Invoke(releaseInfo)));
            }
        }

        private async Task<CloudTable> GetReleaseInfoTableAsync()
        {
            return await _tableStorageService.GetTableAsync(TableName);
        }
    }
}