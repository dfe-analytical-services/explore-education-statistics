using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
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
        private readonly IPublisherTableStorageService _publisherTableStorageService;

        public ReleasePublishingStatusService(
            ContentDbContext context,
            ILogger<ReleasePublishingStatusService> logger,
            IPublisherTableStorageService publisherTableStorageService)
        {
            _context = context;
            _logger = logger;
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
                .Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
                .FirstAsync(rv => rv.Id == releasePublishingKey.ReleaseVersionId);

            var releaseStatus = new ReleasePublishingStatus(
                releaseVersionId: releaseVersion.Id,
                releaseStatusId: releasePublishingKey.ReleaseStatusId,
                publicationSlug: releaseVersion.Release.Publication.Slug,
                publish: immediate ? null : releaseVersion.PublishScheduled,
                releaseSlug: releaseVersion.Release.Slug,
                state,
                immediate: immediate,
                logMessages);

            await _publisherTableStorageService.CreateEntity(
                PublisherReleaseStatusTableName,
                releaseStatus);
        }

        public async Task<ReleasePublishingStatus> Get(ReleasePublishingKey releasePublishingKey)
        {
            var result = await _publisherTableStorageService.GetEntityIfExists<ReleasePublishingStatus>(
                tableName: PublisherReleaseStatusTableName,
                partitionKey: releasePublishingKey.ReleaseVersionId.ToString(),
                rowKey: releasePublishingKey.ReleaseStatusId.ToString());

            if (result == null)
            {
                throw new Exception($"""
                                    Failed to fetch entity from PublisherReleaseStatusTable.
                                    ReleaseVersionId/PartitionKey: {releasePublishingKey.ReleaseVersionId}
                                    ReleaseStatusId/RowKey: {releasePublishingKey.ReleaseStatusId}
                                    """);
            }

            return result;
        }

        public async Task<IReadOnlyList<ReleasePublishingKey>> GetWherePublishingDueToday(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            var filter = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                status.Publish < DateTime.Today.AddDays(1));

            filter = UpdateFilterForStages(filter, content, files, publishing, overall);

            var asyncPageable = await _publisherTableStorageService
                .QueryEntities<ReleasePublishingStatus>(
                    PublisherReleaseStatusTableName,
                    filter);

            var tableResults = await asyncPageable.ToListAsync();

            return tableResults
                .Select(status => status.AsTableRowKey())
                .ToList();
        }

        public async Task<IReadOnlyList<ReleasePublishingKey>> GetWherePublishingDueTodayOrInFuture(
            IReadOnlyList<Guid> releaseVersionIds,
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            var filter = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                status.Publish >= DateTime.Today);

            filter = UpdateFilterForStages(filter, content, files, publishing, overall);

            var asyncPageable = await _publisherTableStorageService
                .QueryEntities<ReleasePublishingStatus>(
                    PublisherReleaseStatusTableName,
                    filter);

            var tableResults = await asyncPageable.ToListAsync();

            return tableResults
                .Where(status => releaseVersionIds.Contains(status.ReleaseVersionId))
                .Select(status => status.AsTableRowKey())
                .ToList();
        }

        public async Task<List<ReleasePublishingStatus>> GetAllByOverallStage(
            Guid releaseVersionId,
            params ReleasePublishingStatusOverallStage[] overallStages)
        {
            var filter = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                status.PartitionKey == releaseVersionId.ToString());

            var stageFilter = "";
            foreach (var stage in overallStages)
            {
                var stageStr = Enum.GetName(stage);
                var stageFilterCondition = TableClient.CreateQueryFilter<ReleasePublishingStatus>(status =>
                    status.OverallStage == stageStr);

                stageFilter = stageFilter == ""
                    ? stageFilterCondition
                    : DataTableStorageUtils.CombineQueryFiltersOr(stageFilter, stageFilterCondition);
            }

            filter = stageFilter == ""
                ? filter
                : DataTableStorageUtils.CombineQueryFiltersAnd(filter, stageFilter);

            var results = await _publisherTableStorageService
                .QueryEntities<ReleasePublishingStatus>(
                    PublisherReleaseStatusTableName,
                    filter);

            return await results.ToListAsync();
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

        public async Task UpdateState(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusState state)
        {
            await UpdateWithRetries(releasePublishingKey,
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
            await UpdateWithRetries(releasePublishingKey,
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
            await UpdateWithRetries(releasePublishingKey,
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
            await UpdateWithRetries(releasePublishingKey,
                row =>
                {
                    row.State.Publishing = stage;
                    row.AppendLogMessage(logMessage);
                    return row;
                });
        }

        private async Task UpdateWithRetries(
            ReleasePublishingKey releasePublishingKey,
            Func<ReleasePublishingStatus, ReleasePublishingStatus> updateFunction,
            int numRetries = 5)
        {
            var releasePublishingStatus = await Get(releasePublishingKey);

            var updatedStatus = updateFunction.Invoke(releasePublishingStatus);

            try
            {
                await _publisherTableStorageService.UpdateEntity(
                    PublisherReleaseStatusTableName,
                    updatedStatus
                );
            }
            catch (RequestFailedException e)
            {
                if (e.Status != (int)HttpStatusCode.PreconditionFailed)
                {
                    throw;
                }
                
                // If the ETag of the status fetched doesn't match the one found in the table,
                // something else has updated the entity, so we should refetch the entity and try the update again.
                _logger.LogDebug("Precondition failure. ETag does not match");
                if (numRetries > 0)
                {
                    numRetries--;
                    await UpdateWithRetries(releasePublishingKey,
                        updateFunction,
                        numRetries);
                }
                else
                {
                    throw new Exception("Did not complete Storage table entity update despite retries");
                }
            }
        }
    }
}
