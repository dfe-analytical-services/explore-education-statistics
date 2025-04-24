using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleasePublishingStatusService(
        ContentDbContext context,
        ILogger<ReleasePublishingStatusService> logger,
        IPublisherTableStorageService publisherTableStorageService)
        : IReleasePublishingStatusService
    {
        public async Task Create(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusState state,
            bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage>? logMessages = null)
        {
            var releaseVersion = await context.ReleaseVersions
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

            await publisherTableStorageService.CreateEntity(
                PublisherReleaseStatusTableName,
                releaseStatus);
        }

        public async Task<ReleasePublishingStatus> Get(ReleasePublishingKey releasePublishingKey)
        {
            var result = await publisherTableStorageService.GetEntityIfExists<ReleasePublishingStatus>(
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

        public async Task<IReadOnlyList<ReleasePublishingKey>> GetScheduledReleasesForPublishingRelativeToDate(
            DateComparison comparison,
            DateTimeOffset referenceDate)
        {
            var overallStageFilter = CreateQueryFilter(
                status => status.OverallStage == nameof(ReleasePublishingStatusOverallStage.Scheduled));

            var publishDateFilter = comparison switch
            {
                DateComparison.BeforeOrOn => CreateQueryFilter(status => status.Publish <= referenceDate),
                DateComparison.After => CreateQueryFilter(status => status.Publish > referenceDate),
                _ => throw new ArgumentOutOfRangeException(nameof(comparison), comparison, null)
            };

            var filter = $"{overallStageFilter} and {publishDateFilter}";

            var result = await QueryEntitiesAsTableRowKeys(filter);

            logger.LogInformation(
                "Querying scheduled releases returned {Count} results. Filter: '{Filter}'",
                result.Count,
                filter);

            return result;
        }

        public async Task<IReadOnlyList<ReleasePublishingKey>> GetScheduledReleasesReadyForPublishing()
        {
            // Release versions ready for scheduled publishing have completed the tasks
            // performed by the StageScheduledReleases function and are in the "Started" stage.
            var overallStageFilter = CreateQueryFilter(
                status => status.OverallStage == nameof(ReleasePublishingStatusOverallStage.Started));

            // Match the internal stages with expected values
            var contentStageFilter = CreateQueryFilter(
                status => status.ContentStage == nameof(ReleasePublishingStatusContentStage.Scheduled));
            var filesStageFilter = CreateQueryFilter(
                status => status.FilesStage == nameof(ReleasePublishingStatusFilesStage.Complete));
            var publishingFilter = CreateQueryFilter(
                status => status.PublishingStage == nameof(ReleasePublishingStatusPublishingStage.Scheduled));

            var filter = string.Join(" and ",
                overallStageFilter,
                contentStageFilter,
                filesStageFilter,
                publishingFilter);

            var result = await QueryEntitiesAsTableRowKeys(filter);

            logger.LogInformation(
                "Querying scheduled releases ready for publishing returned {Count} results. Filter: '{Filter}'",
                result.Count,
                filter);

            return result;
        }

        public async Task<IReadOnlyList<ReleasePublishingKey>> GetReleasesWithOverallStages(
            Guid releaseVersionId,
            ReleasePublishingStatusOverallStage[] overallStages)
        {
            if (overallStages.Length == 0)
            {
                throw new ArgumentException("At least one overall stage must be provided.", nameof(overallStages));
            }

            var partitionKeyFilter = CreateQueryFilter(status => status.PartitionKey == releaseVersionId.ToString());

            var stageFilter = string.Join(" or ",
                overallStages.Select(stage => CreateQueryFilter(status => status.OverallStage == stage.ToString())));

            var filter = overallStages.Length == 1
                ? $"{partitionKeyFilter} and {stageFilter}"
                : $"{partitionKeyFilter} and ({stageFilter})";

            return await QueryEntitiesAsTableRowKeys(filter);
        }

        public async Task<ReleasePublishingStatus?> GetLatest(Guid releaseVersionId)
        {
            var asyncPages = await publisherTableStorageService
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
                await publisherTableStorageService.UpdateEntity(
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
                logger.LogDebug("Precondition failure. ETag does not match");
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

        private static string CreateQueryFilter(Expression<Func<ReleasePublishingStatus, bool>> filter) =>
            TableClient.CreateQueryFilter(filter);

        private async Task<IReadOnlyList<ReleasePublishingKey>> QueryEntitiesAsTableRowKeys(string filter)
        {
            var asyncPageable = await publisherTableStorageService
                .QueryEntities<ReleasePublishingStatus>(
                    PublisherReleaseStatusTableName,
                    filter);

            var result = await asyncPageable
                .Select(status => status.AsTableRowKey())
                .ToListAsync();

            return result;
        }
    }

    public enum DateComparison
    {
        BeforeOrOn,
        After
    }
}
