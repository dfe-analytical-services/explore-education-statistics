using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Services.ReleaseStatusTableQueryUtil;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleasePublishingStatusService : IReleasePublishingStatusService
    {
        private readonly ContentDbContext _context;
        private readonly ILogger<ReleasePublishingStatusService> _logger;
        private readonly IPublisherTableStorageService _tableStorageService;

        public ReleasePublishingStatusService(
            ContentDbContext context,
            ILogger<ReleasePublishingStatusService> logger,
            IPublisherTableStorageService tableStorageService)
        {
            _context = context;
            _logger = logger;
            _tableStorageService = tableStorageService;
        }

        public async Task<ReleasePublishingKey> Create(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusState state,
            bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage>? logMessages = null)
        {
            var releaseVersion = await _context.ReleaseVersions
                .AsNoTracking()
                .Include(rv => rv.Publication)
                .FirstAsync(rv => rv.Id == releasePublishingKey.ReleaseVersionId);

            var releaseStatus = new ReleasePublishingStatus(publicationSlug: releaseVersion.Publication.Slug,
                publish: immediate ? null : releaseVersion.PublishScheduled,
                releaseVersionId: releaseVersion.Id,
                releaseStatusId: releasePublishingKey.ReleaseStatusId,
                releaseSlug: releaseVersion.Slug,
                state,
                immediate: immediate,
                logMessages);

            var tableResult = await GetTable().ExecuteAsync(TableOperation.Insert(releaseStatus));
            return (tableResult.Result as ReleasePublishingStatus).AsTableRowKey();
        }

        public async Task<ReleasePublishingStatus> Get(ReleasePublishingKey releasePublishingKey)
        {
            var tableResult = await GetTable().ExecuteAsync(
                TableOperation.Retrieve<ReleasePublishingStatus>(
                    partitionKey: releasePublishingKey.ReleaseVersionId.ToString(),
                    rowkey: releasePublishingKey.ReleaseStatusId.ToString(),
                    [
                        nameof(ReleasePublishingStatus.Created),
                        nameof(ReleasePublishingStatus.PublicationSlug),
                        nameof(ReleasePublishingStatus.Publish),
                        nameof(ReleasePublishingStatus.ReleaseSlug),
                        nameof(ReleasePublishingStatus.ContentStage),
                        nameof(ReleasePublishingStatus.FilesStage),
                        nameof(ReleasePublishingStatus.PublishingStage),
                        nameof(ReleasePublishingStatus.OverallStage),
                        nameof(ReleasePublishingStatus.Immediate),
                        nameof(ReleasePublishingStatus.Messages)
                    ]));

            return tableResult.Result as ReleasePublishingStatus;
        }

        public async Task<IReadOnlyList<ReleasePublishingKey>> GetWherePublishingDueTodayWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            var query = QueryPublishLessThanEndOfTodayWithStages(content, files, publishing, overall);
            var tableResult = await ExecuteQuery(query);
            return tableResult.Select(status => status.AsTableRowKey()).ToList();
        }

        public async Task<IReadOnlyList<ReleasePublishingKey>> GetWherePublishingDueTodayOrInFutureWithStages(
            IReadOnlyList<Guid> releaseVersionIds,
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            var query = QueryPublishTodayOrInFutureWithStages(content, files, publishing, overall);
            var tableResult = await ExecuteQuery(query);
            return tableResult.Where(status => releaseVersionIds.Contains(status.ReleaseVersionId))
                .Select(status => status.AsTableRowKey())
                .ToList();
        }


        public async Task<IReadOnlyList<ReleasePublishingStatus>> GetAllByOverallStage(
            Guid releaseVersionId,
            params ReleasePublishingStatusOverallStage[] overallStages)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatus.PartitionKey),
                QueryComparisons.Equal,
                releaseVersionId.ToString());

            if (overallStages.Any())
            {
                var allStageFilters = overallStages.ToList().Aggregate("",
                    (acc, stage) =>
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
            return await ExecuteQuery(query);
        }

        public async Task<ReleasePublishingStatus?> GetLatest(Guid releaseVersionId)
        {
            var query = new TableQuery<ReleasePublishingStatus>()
                .Where(TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatus.PartitionKey),
                    QueryComparisons.Equal,
                    releaseVersionId.ToString()));

            var result = await ExecuteQuery(query);
            return result.OrderByDescending(status => status.Created).FirstOrDefault();
        }

        private async Task<IReadOnlyList<ReleasePublishingStatus>> ExecuteQuery(
            TableQuery<ReleasePublishingStatus> query)
        {
            return await _tableStorageService.ExecuteQuery(PublisherReleaseStatusTableName, query);
        }

        public async Task UpdateState(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusState state)
        {
            await UpdateRowAsync(releasePublishingKey,
                row =>
                {
                    row.State = state;
                    return row;
                });
        }

        public async Task UpdateContentStage(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusContentStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await UpdateRowAsync(releasePublishingKey,
                row =>
                {
                    row.State.Content = stage;
                    row.AppendLogMessage(logMessage);
                    return row;
                });
        }

        public async Task UpdateFilesStage(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusFilesStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await UpdateRowAsync(releasePublishingKey,
                row =>
                {
                    row.State.Files = stage;
                    row.AppendLogMessage(logMessage);
                    return row;
                });
        }

        public async Task UpdatePublishingStage(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await UpdateRowAsync(releasePublishingKey,
                row =>
                {
                    row.State.Publishing = stage;
                    row.AppendLogMessage(logMessage);
                    return row;
                });
        }

        private async Task UpdateRowAsync(
            ReleasePublishingKey releasePublishingKey,
            Func<ReleasePublishingStatus, ReleasePublishingStatus> updateFunction,
            int retry = 0)
        {
            var table = GetTable();
            var releasePublishingStatus = await Get(releasePublishingKey);

            try
            {
                await table.ExecuteAsync(TableOperation.Replace(updateFunction.Invoke(releasePublishingStatus)));
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
                {
                    _logger.LogDebug("Precondition failure as expected. ETag does not match");
                    if (retry++ < 5)
                    {
                        await UpdateRowAsync(releasePublishingKey,
                            updateFunction,
                            retry);
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

        private CloudTable GetTable()
        {
            return _tableStorageService.GetTable(PublisherReleaseStatusTableName);
        }
    }
}
