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
        private readonly IPublisherTableStorageServiceOld _tableStorageServiceOld; // @MarkFix change

        public ReleasePublishingStatusService(
            ContentDbContext context,
            ILogger<ReleasePublishingStatusService> logger,
            IPublisherTableStorageServiceOld tableStorageServiceOld)
        {
            _context = context;
            _logger = logger;
            _tableStorageServiceOld = tableStorageServiceOld;
        }

        public async Task<ReleasePublishingKeyOld> Create(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            ReleasePublishingStatusState state,
            bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage>? logMessages = null)
        {
            var releaseVersion = await _context.ReleaseVersions
                .AsNoTracking()
                .Include(rv => rv.Publication)
                .FirstAsync(rv => rv.Id == releasePublishingKeyOld.ReleaseVersionId);

            var releaseStatus = new ReleasePublishingStatusOld(publicationSlug: releaseVersion.Publication.Slug,
                publish: immediate ? null : releaseVersion.PublishScheduled,
                releaseVersionId: releaseVersion.Id,
                releaseStatusId: releasePublishingKeyOld.ReleaseStatusId,
                releaseSlug: releaseVersion.Slug,
                state,
                immediate: immediate,
                logMessages);

            var tableResult = await GetTable().ExecuteAsync(TableOperation.Insert(releaseStatus));
            return (tableResult.Result as ReleasePublishingStatusOld).AsTableRowKey();
        }

        public async Task<ReleasePublishingStatusOld> Get(ReleasePublishingKeyOld releasePublishingKeyOld)
        {
            var tableResult = await GetTable().ExecuteAsync(
                TableOperation.Retrieve<ReleasePublishingStatusOld>(
                    partitionKey: releasePublishingKeyOld.ReleaseVersionId.ToString(),
                    rowkey: releasePublishingKeyOld.ReleaseStatusId.ToString(),
                    [
                        nameof(ReleasePublishingStatusOld.Created),
                        nameof(ReleasePublishingStatusOld.PublicationSlug),
                        nameof(ReleasePublishingStatusOld.Publish),
                        nameof(ReleasePublishingStatusOld.ReleaseSlug),
                        nameof(ReleasePublishingStatusOld.ContentStage),
                        nameof(ReleasePublishingStatusOld.FilesStage),
                        nameof(ReleasePublishingStatusOld.PublishingStage),
                        nameof(ReleasePublishingStatusOld.OverallStage),
                        nameof(ReleasePublishingStatusOld.Immediate),
                        nameof(ReleasePublishingStatusOld.Messages)
                    ]));

            return tableResult.Result as ReleasePublishingStatusOld;
        }

        public async Task<IReadOnlyList<ReleasePublishingKeyOld>> GetWherePublishingDueTodayWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            var query = QueryPublishLessThanEndOfTodayWithStages(content, files, publishing, overall);
            var tableResult = await ExecuteQuery(query);
            return tableResult.Select(status => status.AsTableRowKey()).ToList();
        }

        public async Task<IReadOnlyList<ReleasePublishingKeyOld>> GetWherePublishingDueTodayOrInFutureWithStages(
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


        public async Task<IReadOnlyList<ReleasePublishingStatusOld>> GetAllByOverallStage(
            Guid releaseVersionId,
            params ReleasePublishingStatusOverallStage[] overallStages)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatusOld.PartitionKey),
                QueryComparisons.Equal,
                releaseVersionId.ToString());

            if (overallStages.Any())
            {
                var allStageFilters = overallStages.ToList().Aggregate("",
                    (acc, stage) =>
                    {
                        var stageFilter = TableQuery.GenerateFilterCondition(
                            nameof(ReleasePublishingStatusOld.OverallStage),
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

            var query = new TableQuery<ReleasePublishingStatusOld>().Where(filter);
            return await ExecuteQuery(query);
        }

        public async Task<ReleasePublishingStatusOld?> GetLatest(Guid releaseVersionId)
        {
            var query = new TableQuery<ReleasePublishingStatusOld>()
                .Where(TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatusOld.PartitionKey),
                    QueryComparisons.Equal,
                    releaseVersionId.ToString()));

            var result = await ExecuteQuery(query);
            return result.OrderByDescending(status => status.Created).FirstOrDefault();
        }

        private async Task<IReadOnlyList<ReleasePublishingStatusOld>> ExecuteQuery(
            TableQuery<ReleasePublishingStatusOld> query)
        {
            return await _tableStorageServiceOld.ExecuteQuery(PublisherReleaseStatusTableName, query);
        }

        public async Task UpdateState(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            ReleasePublishingStatusState state)
        {
            await UpdateRowAsync(releasePublishingKeyOld,
                row =>
                {
                    row.State = state;
                    return row;
                });
        }

        public async Task UpdateContentStage(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            ReleasePublishingStatusContentStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await UpdateRowAsync(releasePublishingKeyOld,
                row =>
                {
                    row.State.Content = stage;
                    row.AppendLogMessage(logMessage);
                    return row;
                });
        }

        public async Task UpdateFilesStage(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            ReleasePublishingStatusFilesStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await UpdateRowAsync(releasePublishingKeyOld,
                row =>
                {
                    row.State.Files = stage;
                    row.AppendLogMessage(logMessage);
                    return row;
                });
        }

        public async Task UpdatePublishingStage(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null)
        {
            await UpdateRowAsync(releasePublishingKeyOld,
                row =>
                {
                    row.State.Publishing = stage;
                    row.AppendLogMessage(logMessage);
                    return row;
                });
        }

        private async Task UpdateRowAsync(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            Func<ReleasePublishingStatusOld, ReleasePublishingStatusOld> updateFunction,
            int retry = 0)
        {
            var table = GetTable();
            var releasePublishingStatus = await Get(releasePublishingKeyOld);

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
                        await UpdateRowAsync(releasePublishingKeyOld,
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
            return _tableStorageServiceOld.GetTable(PublisherReleaseStatusTableName);
        }
    }
}
