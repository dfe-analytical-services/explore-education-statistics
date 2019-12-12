using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseStatusService : IReleaseStatusService
    {
        private readonly ITableStorageService _tableStorageService;
        private const string TableName = "ReleaseStatus";

        public ReleaseStatusService(ITableStorageService tableStorageService)
        {
            _tableStorageService = tableStorageService;
        }

        public async Task AddAsync(string publicationSlug, DateTime publish, Guid releaseId, string releaseSlug,
            Stage stage)
        {
            var table = await GetTableAsync();
            await table.ExecuteAsync(TableOperation.InsertOrReplace(new ReleaseStatus(publicationSlug,
                publish,
                releaseId,
                releaseSlug,
                stage)));
        }

        public async Task<IEnumerable<ReleaseStatus>> ExecuteQueryAsync(TableQuery<ReleaseStatus> query)
        {
            var results = new List<ReleaseStatus>();
            var table = await GetTableAsync();
            TableContinuationToken token = null;
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(query, token);
                results.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return results;
        }

        public async Task UpdateContentStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.ContentStage = stage.ToString();
                return FailReleaseIfTaskStageFailed(row);
            });
        }

        public async Task UpdateDataStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.DataStage = stage.ToString();
                return FailReleaseIfTaskStageFailed(row);
            });
        }

        public async Task UpdateFilesStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.FilesStage = stage.ToString();
                return FailReleaseIfTaskStageFailed(row);
            });
        }

        public async Task UpdateStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.Stage = stage.ToString();
                return row;
            });
        }

        private async Task UpdateRowAsync(Guid releaseId, Guid releaseStatusId,
            Func<ReleaseStatus, ReleaseStatus> updateFunction)
        {
            var table = await GetTableAsync();
            var tableResult = await table.ExecuteAsync(
                TableOperation.Retrieve<ReleaseStatus>(releaseId.ToString(), releaseStatusId.ToString(),
                    new List<string>
                    {
                        "Created",
                        "PublicationSlug",
                        "Publish",
                        "ReleaseSlug",
                        "ContentStage",
                        "FilesStage",
                        "DataStage",
                        "Stage"
                    }));

            if (tableResult.Result is ReleaseStatus releaseStatus)
            {
                await table.ExecuteAsync(TableOperation.InsertOrMerge(updateFunction.Invoke(releaseStatus)));
            }
        }

        private ReleaseStatus FailReleaseIfTaskStageFailed(ReleaseStatus releaseStatus)
        {
            if (Enum.TryParse<Stage>(releaseStatus.Stage, out var stage) && stage != Failed)
            {
                if (Enum.TryParse<Stage>(releaseStatus.ContentStage, out var contentStage) && contentStage == Failed ||
                    Enum.TryParse<Stage>(releaseStatus.DataStage, out var dataStage) && dataStage == Failed ||
                    Enum.TryParse<Stage>(releaseStatus.FilesStage, out var filesStage) && filesStage == Failed)
                {
                    releaseStatus.Stage = Failed.ToString();
                }
            }

            return releaseStatus;
        }

        private async Task<CloudTable> GetTableAsync()
        {
            return await _tableStorageService.GetTableAsync(TableName);
        }
    }
}