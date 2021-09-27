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
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Services.ReleaseStatusTableQueryUtil;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleasePublishingStatusService : IReleasePublishingStatusService
    {
        private readonly ContentDbContext _context;
        private readonly ILogger<ReleasePublishingStatusService> _logger;
        private readonly ITableStorageService _tableStorageService;

        public ReleasePublishingStatusService(ContentDbContext context,
            ILogger<ReleasePublishingStatusService> logger,
            ITableStorageService tableStorageService)
        {
            _context = context;
            _logger = logger;
            _tableStorageService = tableStorageService;
        }

        public async Task<ReleasePublishingStatus> CreateAsync(Guid releaseId, Guid releaseStatusId, ReleasePublishingStatusState state, bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage> logMessages = null)
        {
            var release = await GetReleaseAsync(releaseId);
            var table = await GetTableAsync();
            var publish = immediate ? null : release.PublishScheduled;
            var releaseStatus = new ReleasePublishingStatus(release.Publication.Slug, publish, release.Id,
                releaseStatusId, release.Slug, state, immediate, logMessages);
            var tableResult = await table.ExecuteAsync(TableOperation.Insert(releaseStatus));
            return tableResult.Result as ReleasePublishingStatus;
        }

        public async Task<ReleasePublishingStatus> GetAsync(Guid releaseId, Guid releaseStatusId)
        {
            var table = await GetTableAsync();
            var tableResult = await table.ExecuteAsync(
                TableOperation.Retrieve<ReleasePublishingStatus>(releaseId.ToString(), releaseStatusId.ToString(),
                    new List<string>
                    {
                        nameof(ReleasePublishingStatus.Created),
                        nameof(ReleasePublishingStatus.PublicationSlug),
                        nameof(ReleasePublishingStatus.Publish),
                        nameof(ReleasePublishingStatus.ReleaseSlug),
                        nameof(ReleasePublishingStatus.ContentStage),
                        nameof(ReleasePublishingStatus.DataStage),
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
            ReleasePublishingStatusDataStage? data = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null)
        {
            var query = QueryPublishLessThanEndOfTodayWithStages(content, data, files, publishing, overall);
            return await ExecuteQueryAsync(query);
        }

        public async Task<IEnumerable<ReleasePublishingStatus>> GetAllByOverallStage(Guid releaseId, params ReleasePublishingStatusOverallStage[] overallStages)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatus.PartitionKey),
                    QueryComparisons.Equal, releaseId.ToString());

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

        public async Task<ReleasePublishingStatus> GetLatestAsync(Guid releaseId)
        {
            var query = new TableQuery<ReleasePublishingStatus>()
                .Where(TableQuery.GenerateFilterCondition(nameof(ReleasePublishingStatus.PartitionKey),
                    QueryComparisons.Equal, releaseId.ToString()));

            var result = await ExecuteQueryAsync(query);
            return result.OrderByDescending(releaseStatus => releaseStatus.Created).FirstOrDefault();
        }

        public async Task<bool> IsImmediate(Guid releaseId, Guid releaseStatusId)
        {
            var releaseStatus = await GetAsync(releaseId, releaseStatusId);
            return releaseStatus.Immediate;
        }

        private Task<IEnumerable<ReleasePublishingStatus>> ExecuteQueryAsync(TableQuery<ReleasePublishingStatus> query)
        {
            return _tableStorageService.ExecuteQueryAsync(PublisherReleaseStatusTableName, query);
        }

        public async Task UpdateStateAsync(Guid releaseId, Guid releaseStatusId, ReleasePublishingStatusState state)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State = state;
                return row;
            });
        }

        public async Task UpdateStagesAsync(Guid releaseId, Guid releaseStatusId, ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusDataStage? data = null, ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null, ReleasePublishingStatusOverallStage? overall = null,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                if (content.HasValue)
                {
                    row.State.Content = content.Value;
                }

                if (data.HasValue)
                {
                    row.State.Data = data.Value;
                }

                if (files.HasValue)
                {
                    row.State.Files = files.Value;
                }

                if (publishing.HasValue)
                {
                    row.State.Publishing = publishing.Value;
                }
                
                if (overall.HasValue)
                {
                    row.State.Overall = overall.Value;
                }

                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdateContentStageAsync(Guid releaseId, Guid releaseStatusId, ReleasePublishingStatusContentStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Content = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdateDataStageAsync(Guid releaseId, Guid releaseStatusId, ReleasePublishingStatusDataStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Data = stage;

                if (stage == ReleasePublishingStatusDataStage.Complete)
                {
                    // In case this was a retry, reinstate publishing if possible
                    var (publishing, overall) = GetStatesForReinstatingAfterPossibleRetry(row);
                    row.State.Publishing = publishing;
                    row.State.Overall = overall;
                }

                row.AppendLogMessage(logMessage);
                return row;
            });

            if (stage == ReleasePublishingStatusDataStage.Failed)
            {
                await CancelReleasesWithContentDependency(releaseId, releaseStatusId);
            }

            if (stage == ReleasePublishingStatusDataStage.Complete)
            {
                // In case this was a retry, reinstate publishing of any other releases that were cancelled if possible
                await ReinstateReleasesWithContentDependency(releaseId, releaseStatusId);
            }
        }

        public async Task UpdateFilesStageAsync(Guid releaseId, Guid releaseStatusId, ReleasePublishingStatusFilesStage stage,
            ReleasePublishingStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Files = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        public async Task UpdatePublishingStageAsync(Guid releaseId, Guid releaseStatusId,
            ReleasePublishingStatusPublishingStage stage, ReleasePublishingStatusLogMessage logMessage = null)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State.Publishing = stage;
                row.AppendLogMessage(logMessage);
                return row;
            });
        }

        private async Task UpdateRowAsync(Guid releaseId, Guid releaseStatusId,
            Func<ReleasePublishingStatus, ReleasePublishingStatus> updateFunction, int retry = 0)
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
            return await _tableStorageService.GetTableAsync(PublisherReleaseStatusTableName);
        }

        /// <summary>
        /// Cancel publishing of any other Releases sharing a dependency with the content of a failing Release.
        /// Publishing any Release copies the whole staging directory.
        /// Cancelling others is necessary to avoid publishing staged content of a Release which is now failing,
        /// and other parts of content which were calculated expecting that Release to exist.
        /// </summary>
        /// <param name="failingReleaseId"></param>
        /// <param name="failingReleaseStatusId"></param>
        /// <returns></returns>
        private async Task CancelReleasesWithContentDependency(Guid failingReleaseId, Guid failingReleaseStatusId)
        {
            if (await IsImmediate(failingReleaseId, failingReleaseStatusId))
            {
                return;
            }
            
            var scheduled = await GetWherePublishingDueTodayWithStages(
                publishing: ReleasePublishingStatusPublishingStage.Scheduled);
            var scheduledExceptFailing = scheduled
                .Where(status => status.Id != failingReleaseStatusId);

            foreach (var releaseStatus in scheduledExceptFailing)
            {
                await UpdateStagesAsync(releaseStatus.ReleaseId, releaseStatus.Id,
                    publishing: ReleasePublishingStatusPublishingStage.Cancelled, 
                    overall: ReleasePublishingStatusOverallStage.Failed,
                    logMessage: new ReleasePublishingStatusLogMessage(
                        $"Publishing cancelled due to dependency with failed Release: {failingReleaseId}, ReleaseStatusId: {failingReleaseStatusId}"));
            }
        }

        /// <summary>
        /// Reinstates publishing of any other releases sharing a dependency with the content of a Release.
        /// Use this when a Release stage may have failed previously and has now been successful.
        /// </summary>
        /// <param name="succeedingReleaseId"></param>
        /// <param name="succeedingReleaseStatusId"></param>
        /// <returns></returns>
        private async Task ReinstateReleasesWithContentDependency(Guid succeedingReleaseId,
            Guid succeedingReleaseStatusId)
        {
            if (await IsImmediate(succeedingReleaseId, succeedingReleaseStatusId))
            {
                return;
            }

            var scheduled = await GetWherePublishingDueTodayWithStages(
                content: ReleasePublishingStatusContentStage.Complete,
                data: ReleasePublishingStatusDataStage.Complete,
                files: ReleasePublishingStatusFilesStage.Complete,
                publishing: ReleasePublishingStatusPublishingStage.Cancelled,
                overall: ReleasePublishingStatusOverallStage.Failed);

            var scheduledExceptSucceeding = scheduled
                .Where(status => status.Id != succeedingReleaseStatusId);

            foreach (var releaseStatus in scheduledExceptSucceeding)
            {
                var (publishing, overall) = GetStatesForReinstatingAfterPossibleRetry(releaseStatus);

                await UpdateStagesAsync(releaseStatus.ReleaseId, releaseStatus.Id,
                    publishing: publishing,
                    overall: overall,
                    logMessage: new ReleasePublishingStatusLogMessage(
                        $"Publishing reinstated after Release succeeded: {succeedingReleaseId}, ReleaseStatusId: {succeedingReleaseStatusId}"));
            }
        }

        /// <summary>
        /// Get the states for reinstating stages after they may have been cancelled due to a failure.
        /// </summary>
        /// <remarks>
        /// Checks that all the states are as expected and if not, leaves them untouched.
        /// </remarks>
        /// <param name="releasePublishingStatus"></param>
        /// <returns>Returns the original starting states for stages before they were cancelled, if reinstating is possible.</returns>
        private static (ReleasePublishingStatusPublishingStage Publishing, ReleasePublishingStatusOverallStage Overall) 
            GetStatesForReinstatingAfterPossibleRetry(ReleasePublishingStatus releasePublishingStatus)
        {
            var startingState = releasePublishingStatus.Immediate
                ? ReleasePublishingStatusStates.ImmediateReleaseStartedState
                : ReleasePublishingStatusStates.ScheduledReleaseStartedState;

            var existingState = releasePublishingStatus.State;

            var publishingCancelled = existingState.Publishing == ReleasePublishingStatusPublishingStage.Cancelled;
            var overallFailed = existingState.Overall == ReleasePublishingStatusOverallStage.Failed;

            var contentOk = existingState.Content == ReleasePublishingStatusContentStage.Complete 
                            || releasePublishingStatus.Immediate && existingState.Content == startingState.Content;
            var filesOk = existingState.Files == ReleasePublishingStatusFilesStage.Complete;
            var dataOk = existingState.Data == ReleasePublishingStatusDataStage.Complete;

            var reinstate = publishingCancelled
                            && overallFailed
                            && contentOk
                            && filesOk
                            && dataOk;

            var newPublishingState = reinstate
                 ? startingState.Publishing
                 : existingState.Publishing;

            var newOverallState = reinstate
                ? startingState.Overall
                : existingState.Overall;

            return (newPublishingState, newOverallState);
        }
    }
}
