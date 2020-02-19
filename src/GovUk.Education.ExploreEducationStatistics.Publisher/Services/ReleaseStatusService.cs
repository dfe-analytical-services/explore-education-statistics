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

        public async Task CreateOrUpdateAsync(Guid releaseId, ReleaseStatusState state,
            IEnumerable<ReleaseStatusLogMessage> logMessages = null)
        {
            var release = await GetReleaseAsync(releaseId);
            var table = await GetTableAsync();

            var releaseStatus = await GetReleaseStatus(releaseId);
            if (releaseStatus == null)
            {
                releaseStatus = new ReleaseStatus(release.Publication.Slug, release.PublishScheduled, release.Id,
                    release.Slug, state, logMessages);
            }
            else
            {
                releaseStatus.PublicationSlug = release.Publication.Slug;
                releaseStatus.Publish = release.PublishScheduled;
                releaseStatus.ReleaseSlug = release.Slug;
                releaseStatus.State = state;
                releaseStatus.AppendLogMessages(logMessages);
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

        public async Task UpdateStateAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusState state)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State = state;
                return row;
            });
        }

        public async Task UpdateContentStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Content = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdateDataStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Data = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdateFilesStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage,
            ReleaseStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Files = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdatePublishingStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage,
            ReleaseStatusLogMessage logMessage = null)
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
                        nameof(ReleaseStatus.Messages)
                    }));

            if (tableResult.Result is ReleaseStatus releaseStatus)
            {
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
        }

        private async Task<ReleaseStatus> GetReleaseStatus(Guid releaseId)
        {
            var table = await GetTableAsync();
            var queryResults = table.ExecuteQuery(new TableQuery<ReleaseStatus>().Where(
                TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PartitionKey), QueryComparisons.Equal,
                    releaseId.ToString())));
            return queryResults.SingleOrDefault();
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