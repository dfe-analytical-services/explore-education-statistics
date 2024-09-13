using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Data.Tables;
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
        private readonly IPublisherTableStorageServiceOld _tableStorageServiceOld; // @MarkFix remove
        private readonly IPublisherTableStorageService _publisherTableStorageService;

        public ReleasePublishingStatusService(
            ContentDbContext context,
            ILogger<ReleasePublishingStatusService> logger,
            IPublisherTableStorageServiceOld tableStorageServiceOld,
            IPublisherTableStorageService publisherTableStorageService)
        {
            _context = context;
            _logger = logger;
            _tableStorageServiceOld = tableStorageServiceOld;
            _publisherTableStorageService = publisherTableStorageService;
        }

        public async Task Create(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusState state,
            bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage>? logMessages = null)
        {
            var releaseVersion = await _context.ReleaseVersions
                .AsNoTracking()
                .Include(rv => rv.Publication)
                .FirstAsync(rv => rv.Id == releasePublishingKey.ReleaseVersionId);

            var releaseStatus = new ReleasePublishingStatus(
                releaseVersionId: releaseVersion.Id,
                releaseStatusId: releasePublishingKey.ReleaseStatusId,
                publicationSlug: releaseVersion.Publication.Slug,
                publish: immediate ? null : releaseVersion.PublishScheduled,
                releaseSlug: releaseVersion.Slug,
                state,
                immediate: immediate,
                logMessages);

            await _publisherTableStorageService.CreateEntity(
                PublisherReleaseStatusTableName,
                releaseStatus);
        }

        public async Task<ReleasePublishingStatus> Get(ReleasePublishingKey releasePublishingKey)
        {
            var select = new List<string>
            {
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
            };
            var result = await _publisherTableStorageService.GetEntityIfExists<ReleasePublishingStatus>(
                tableName: PublisherReleaseStatusTableName,
                partitionKey: releasePublishingKey.ReleaseVersionId.ToString(),
                rowKey: releasePublishingKey.ReleaseStatusId.ToString(),
                select: select);

            if (result == null)
            {
                throw new Exception("Failed to fetch entity from PublisherReleaseStatusTable"); // @MarkFix
            }

            return result;
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

        public async Task<ReleasePublishingStatus?> GetLatest(Guid releaseVersionId)
        {
            var asyncPages = await _publisherTableStorageService
                .QueryEntities<ReleasePublishingStatus>(
                    PublisherReleaseStatusTableName,
                    status => status.PartitionKey == releaseVersionId.ToString());
            var statusList = await asyncPages.ToListAsync();

            return statusList.OrderByDescending(status => status.Created).FirstOrDefault();
        }

        private async Task<IReadOnlyList<ReleasePublishingStatusOld>> ExecuteQuery(
            TableQuery<ReleasePublishingStatusOld> query)
        {
            return await _tableStorageServiceOld.ExecuteQuery(PublisherReleaseStatusTableName, query);
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
            int numRetries = 5)
        {
            var releasePublishingStatus = await Get(releasePublishingKey)
                                          ?? throw new Exception("Unable to retrieve release publishing status");

            var updatedStatus = updateFunction.Invoke(releasePublishingStatus);

            try
            {
                await _publisherTableStorageService.UpdateEntity(
                    PublisherReleaseStatusTableName,
                    updatedStatus
                );
            }
            catch (TableTransactionFailedException e) // @MarkFix test retry logic to this if that actually works - I've randomly chosen an exception here
            {
                // @MarkFix write a comment explaining why this code is necessary
                if (e.Status == (int)HttpStatusCode.PreconditionFailed)
                {
                    _logger.LogDebug("Precondition failure as expected. ETag does not match");
                    if (numRetries > 0)
                    {
                        numRetries--;
                        await UpdateRowAsync(releasePublishingKey,
                            updateFunction,
                            numRetries);
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
