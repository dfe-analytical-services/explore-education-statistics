using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseStatusService : IReleaseStatusService
    {
        private readonly ContentDbContext _context;
        private readonly ILogger<ReleaseStatusService> _logger;
        private readonly ITableStorageService _tableStorageService;
        private const string TableName = "ReleaseStatus";

        public ReleaseStatusService(ContentDbContext context,
            ILogger<ReleaseStatusService> logger,
            ITableStorageService tableStorageService)
        {
            _context = context;
            _logger = logger;
            _tableStorageService = tableStorageService;
        }

        public async Task<ReleaseStatus> CreateAsync(Guid releaseId, ReleaseStatusState state, bool immediate,
            IEnumerable<ReleaseStatusLogMessage> logMessages = null)
        {
            var release = await GetReleaseAsync(releaseId);
            var table = await GetTableAsync();
            var publish = immediate ? null : release.PublishScheduled;
            var releaseStatus = new ReleaseStatus(release.Publication.Slug, publish, release.Id,
                release.Slug, state, immediate, logMessages);
            var tableResult = await table.ExecuteAsync(TableOperation.Insert(releaseStatus));
            return tableResult.Result as ReleaseStatus;
        }

        public async Task<ReleaseStatus> GetAsync(Guid releaseId, Guid releaseStatusId)
        {
            var table = await GetTableAsync();
            var tableResult = await table.ExecuteAsync(
                TableOperation.Retrieve<ReleaseStatus>(releaseId.ToString(), releaseStatusId.ToString(),
                    new List<string>
                    {
                        nameof(ReleaseStatus.Created),
                        nameof(ReleaseStatus.PublicationSlug),
                        nameof(ReleaseStatus.Publish),
                        nameof(ReleaseStatus.ReleaseSlug),
                        nameof(ReleaseStatus.ContentStage),
                        nameof(ReleaseStatus.DataStage),
                        nameof(ReleaseStatus.FilesStage),
                        nameof(ReleaseStatus.PublishingStage),
                        nameof(ReleaseStatus.OverallStage),
                        nameof(ReleaseStatus.Immediate),
                        nameof(ReleaseStatus.Messages)
                    }));

            return tableResult.Result as ReleaseStatus;
        }

        public Task<IEnumerable<ReleaseStatus>> GetAllAsync(Guid releaseId, ReleaseStatusOverallStage? overallStage)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PartitionKey),
                    QueryComparisons.Equal, releaseId.ToString());

            if (overallStage.HasValue)
            {
                var stageFilter = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.OverallStage),
                    QueryComparisons.Equal, overallStage.Value.ToString());
                
                filter = TableQuery.CombineFilters(filter, TableOperators.And,
                    stageFilter);
            }

            var query = new TableQuery<ReleaseStatus>().Where(filter);
            return _tableStorageService.ExecuteQueryAsync(TableName, query);
        }

        public async Task<ReleaseStatus> GetLatestAsync(Guid releaseId)
        {
            var query = new TableQuery<ReleaseStatus>()
                .Where(TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PartitionKey),
                    QueryComparisons.Equal, releaseId.ToString()));

            var result = await _tableStorageService.ExecuteQueryAsync(TableName, query);
            return result.OrderByDescending(releaseStatus => releaseStatus.Created).FirstOrDefault();
        }

        public async Task<bool> IsImmediate(Guid releaseId, Guid releaseStatusId)
        {
            var releaseStatus = await GetAsync(releaseId, releaseStatusId);
            return releaseStatus.Immediate;
        }

        public Task<IEnumerable<ReleaseStatus>> ExecuteQueryAsync(TableQuery<ReleaseStatus> query)
        {
            return _tableStorageService.ExecuteQueryAsync(TableName, query);
        }

        public async Task UpdateStateAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusState state)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State = state;
                return row;
            });
        }

        public async Task UpdateStagesAsync(Guid releaseId, Guid releaseStatusId,
            ReleaseStatusContentStage? contentStage,
            ReleaseStatusDataStage? dataStage, ReleaseStatusFilesStage? filesStage,
            ReleaseStatusPublishingStage? publishingStage, ReleaseStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                if (contentStage.HasValue)
                {
                    row.State.Content = contentStage.Value;
                }

                if (dataStage.HasValue)
                {
                    row.State.Data = dataStage.Value;
                }

                if (filesStage.HasValue)
                {
                    row.State.Files = filesStage.Value;
                }

                if (publishingStage.HasValue)
                {
                    row.State.Publishing = publishingStage.Value;
                }

                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdateContentStageAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusContentStage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Content = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdateDataStageAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusDataStage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Data = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdateFilesStageAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusFilesStage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Files = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdatePublishingStageAsync(Guid releaseId, Guid releaseStatusId,
            ReleaseStatusPublishingStage stage, ReleaseStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Publishing = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        private async Task UpdateRowAsync(Guid releaseId, Guid releaseStatusId,
            Func<ReleaseStatus, ReleaseStatus> updateFunction, int retry = 0)
        {
            var table = await GetTableAsync();
            var releaseStatus = await GetAsync(releaseId, releaseStatusId);

            try
            {
                await table.ExecuteAsync(TableOperation.Replace(updateFunction.Invoke(releaseStatus)));
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == (int) HttpStatusCode.PreconditionFailed)
                {
                    _logger.LogDebug("Precondition failure as expected. ETag does not match");
                    if (retry++ < 5)
                    {
                        await UpdateRowAsync(releaseId, releaseStatusId, updateFunction, retry);
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }
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