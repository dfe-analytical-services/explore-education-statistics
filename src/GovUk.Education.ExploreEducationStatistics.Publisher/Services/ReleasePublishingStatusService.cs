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

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class ReleasePublishingStatusService : IReleasePublishingStatusService
{
    private readonly ContentDbContext _context;
    private readonly ILogger<ReleasePublishingStatusService> _logger;
    private readonly IPublisherTableStorageService _tableStorageService;

    public ReleasePublishingStatusService(ContentDbContext context,
        ILogger<ReleasePublishingStatusService> logger,
        IPublisherTableStorageService tableStorageService)
    {
        _context = context;
        _logger = logger;
        _tableStorageService = tableStorageService;
    }

    public async Task<ReleasePublishingStatus> CreateAsync(
        Guid releaseVersionId,
        Guid releaseStatusId,
        ReleasePublishingStatusState state,
        bool immediate,
        IEnumerable<ReleasePublishingStatusLogMessage>? logMessages = null)
    {
        var releaseVersion = await _context.ReleaseVersions
            .AsNoTracking()
            .Include(rv => rv.Publication)
            .FirstAsync(rv => rv.Id == releaseVersionId);

        var releaseStatus = new ReleasePublishingStatus(publicationSlug: releaseVersion.Publication.Slug,
            publish: immediate ? null : releaseVersion.PublishScheduled,
            releaseVersionId: releaseVersion.Id,
            releaseStatusId: releaseStatusId,
            releaseSlug: releaseVersion.Slug,
            state,
            immediate: immediate,
            logMessages);

        var tableResult = await GetTable().ExecuteAsync(TableOperation.Insert(releaseStatus));
        return tableResult.Result as ReleasePublishingStatus;
    }

    public async Task<ReleasePublishingStatus> GetAsync(Guid releaseVersionId,
        Guid releaseStatusId)
    {
        var tableResult = await GetTable().ExecuteAsync(
            TableOperation.Retrieve<ReleasePublishingStatus>(releaseVersionId.ToString(),
                releaseStatusId.ToString(),
                new List<string>
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
                }));

        return tableResult.Result as ReleasePublishingStatus;
    }

    public async Task<IEnumerable<ReleasePublishingStatus>> GetWherePublishingDueTodayWithStages(
        ReleasePublishingStatusContentStage? content = null,
        ReleasePublishingStatusFilesStage? files = null,
        ReleasePublishingStatusPublishingStage? publishing = null,
        ReleasePublishingStatusOverallStage? overall = null)
    {
        var query = QueryPublishLessThanEndOfTodayWithStages(content, files, publishing, overall);
        return await ExecuteQueryAsync(query);
    }

    public async Task<IEnumerable<ReleasePublishingStatus>> GetWherePublishingDueTodayOrInFutureWithStages(
        ReleasePublishingStatusContentStage? content = null,
        ReleasePublishingStatusFilesStage? files = null,
        ReleasePublishingStatusPublishingStage? publishing = null,
        ReleasePublishingStatusOverallStage? overall = null)
    {
        var query = QueryPublishTodayOrInFutureWithStages(content, files, publishing, overall);
        return await ExecuteQueryAsync(query);
    }


    public async Task<IEnumerable<ReleasePublishingStatus>> GetAllByOverallStage(Guid releaseVersionId,
        params ReleasePublishingStatusOverallStage[] overallStages)
    {
        var filter = TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatus.PartitionKey),
            QueryComparisons.Equal, releaseVersionId.ToString());

        if (overallStages.Any())
        {
            var allStageFilters = overallStages.ToList().Aggregate("", (acc, stage) =>
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
        return await ExecuteQueryAsync(query);
    }

    public async Task<ReleasePublishingStatus?> GetLatestAsync(Guid releaseVersionId)
    {
        var query = new TableQuery<ReleasePublishingStatus>()
            .Where(TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatus.PartitionKey),
                QueryComparisons.Equal, releaseVersionId.ToString()));

        var result = await ExecuteQueryAsync(query);
        return result.OrderByDescending(releaseStatus => releaseStatus.Created).FirstOrDefault();
    }

    private Task<IEnumerable<ReleasePublishingStatus>> ExecuteQueryAsync(TableQuery<ReleasePublishingStatus> query)
    {
        return _tableStorageService.ExecuteQueryAsync(PublisherReleaseStatusTableName, query);
    }

    public async Task UpdateStateAsync(Guid releaseVersionId,
        Guid releaseStatusId,
        ReleasePublishingStatusState state)
    {
        await UpdateRowAsync(releaseVersionId: releaseVersionId,
            releaseStatusId: releaseStatusId,
            row =>
        {
            row.State = state;
            return row;
        });
    }

    public async Task UpdateContentStageAsync(
        Guid releaseVersionId,
        Guid releaseStatusId,
        ReleasePublishingStatusContentStage stage,
        ReleasePublishingStatusLogMessage? logMessage = null)
    {
        await UpdateRowAsync(releaseVersionId: releaseVersionId,
            releaseStatusId: releaseStatusId,
            row =>
        {
            row.State.Content = stage;
            row.AppendLogMessage(logMessage);
            return row;
        });
    }

    public async Task UpdateFilesStageAsync(Guid releaseVersionId,
        Guid releaseStatusId,
        ReleasePublishingStatusFilesStage stage,
        ReleasePublishingStatusLogMessage? logMessage = null)
    {
        await UpdateRowAsync(releaseVersionId: releaseVersionId,
            releaseStatusId: releaseStatusId,
            row =>
        {
            row.State.Files = stage;
            row.AppendLogMessage(logMessage);
            return row;
        });
    }

    public async Task UpdatePublishingStageAsync(Guid releaseVersionId,
        Guid releaseStatusId,
        ReleasePublishingStatusPublishingStage stage,
        ReleasePublishingStatusLogMessage? logMessage = null)
    {
        await UpdateRowAsync(releaseVersionId: releaseVersionId,
            releaseStatusId: releaseStatusId,
            row =>
        {
            row.State.Publishing = stage;
            row.AppendLogMessage(logMessage);
            return row;
        });
    }

    private async Task UpdateRowAsync(
        Guid releaseVersionId,
        Guid releaseStatusId,
        Func<ReleasePublishingStatus, ReleasePublishingStatus> updateFunction,
        int retry = 0)
    {
        var table = GetTable();
        var releaseStatus = await GetAsync(releaseVersionId: releaseVersionId,
            releaseStatusId: releaseStatusId);

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
                    await UpdateRowAsync(releaseVersionId: releaseVersionId,
                        releaseStatusId: releaseStatusId,
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
