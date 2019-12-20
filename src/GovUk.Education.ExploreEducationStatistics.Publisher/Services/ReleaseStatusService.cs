using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseStatusService : IReleaseStatusService
    {
        private readonly ContentDbContext _context;
        private readonly ITableStorageService _tableStorageService;
        private const string TableName = "ReleaseStatus";

        public ReleaseStatusService(ContentDbContext context,
            ITableStorageService tableStorageService)
        {
            _context = context;
            _tableStorageService = tableStorageService;
        }

        public async Task CreateOrUpdateAsync(Guid releaseId,
            (Stage Content, Stage Files, Stage Data, Stage Overall) stage,
            IEnumerable<ReleaseStatusLogMessage> logMessages = null)
        {
            var release = await GetReleaseAsync(releaseId);
            var table = await GetTableAsync();

            var releaseStatus = await GetReleaseStatus(releaseId);
            if (releaseStatus == null)
            {
                releaseStatus = new ReleaseStatus(release.Publication.Slug, release.PublishScheduled, release.Id,
                    release.Slug, stage, logMessages);
            }
            else
            {
                releaseStatus.PublicationSlug = release.Publication.Slug;
                releaseStatus.Publish = release.PublishScheduled;
                releaseStatus.ReleaseSlug = release.Slug;
                releaseStatus.ContentStage = stage.Content.ToString();
                releaseStatus.FilesStage = stage.Files.ToString();
                releaseStatus.DataStage = stage.Data.ToString();
                releaseStatus.Stage = stage.Overall.ToString();

                if (logMessages != null)
                {
                    releaseStatus.AppendLogMessages(logMessages);
                }
            }

            await table.ExecuteAsync(TableOperation.InsertOrReplace(releaseStatus));
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

        public async Task UpdateStageAsync(Guid releaseId, Guid releaseStatusId,
            (Stage Content, Stage Files, Stage Data, Stage Overall) stage)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.ContentStage = stage.Content.ToString();
                row.DataStage = stage.Data.ToString();
                row.FilesStage = stage.Files.ToString();
                row.Stage = stage.Overall.ToString();
                return row;
            });
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

        private async Task<ReleaseStatus> GetReleaseStatus(Guid releaseId)
        {
            var table = await GetTableAsync();
            var queryResults = table.ExecuteQuery(new TableQuery<ReleaseStatus>().Where(
                TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PartitionKey), QueryComparisons.Equal,
                    releaseId.ToString())));
            return queryResults.SingleOrDefault();
        }

        private static ReleaseStatus FailReleaseIfTaskStageFailed(ReleaseStatus releaseStatus)
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

        private Task<Release> GetReleaseAsync(Guid releaseId)
        {
            return _context.Releases
                .AsNoTracking()
                .Include(release => release.Publication)
                .SingleAsync(release => release.Id == releaseId);
        }

        private async Task<CloudTable> GetTableAsync()
        {
            return await _tableStorageService.GetTableAsync(TableName);
        }
    }
}