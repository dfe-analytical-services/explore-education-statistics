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
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ReleaseStatusService : IReleaseStatusService
    {
        private readonly ContentDbContext _context;
        private readonly ILogger<ReleaseStatusService> _logger;
        private readonly ITableStorageService _tableStorageService;

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

        public async Task<IEnumerable<ReleaseStatus>> GetWherePublishingDueTodayWithStages(
            ReleaseStatusContentStage? content = null,
            ReleaseStatusDataStage? data = null,
            ReleaseStatusFilesStage? files = null,
            ReleaseStatusPublishingStage? publishing = null,
            ReleaseStatusOverallStage? overall = null)
        {
            var query = QueryPublishLessThanEndOfTodayWithStages(content, data, files, publishing, overall);
            return await ExecuteQueryAsync(query);
        }

        public async Task<IEnumerable<ReleaseStatus>> GetAllByOverallStage(Guid releaseId, params ReleaseStatusOverallStage[] overallStages)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PartitionKey),
                    QueryComparisons.Equal, releaseId.ToString());

            if (overallStages.Any())
            {
                var allStageFilters = overallStages.ToList().Aggregate("", (acc, stage) =>
                {
                    var stageFilter = TableQuery.GenerateFilterCondition(
                        nameof(ReleaseStatus.OverallStage),
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

            var query = new TableQuery<ReleaseStatus>().Where(filter);
            return await ExecuteQueryAsync(query);
        }

        public async Task<ReleaseStatus> GetLatestAsync(Guid releaseId)
        {
            var query = new TableQuery<ReleaseStatus>()
                .Where(TableQuery.GenerateFilterCondition(nameof(ReleaseStatus.PartitionKey),
                    QueryComparisons.Equal, releaseId.ToString()));

            var result = await ExecuteQueryAsync(query);
            return result.OrderByDescending(releaseStatus => releaseStatus.Created).FirstOrDefault();
        }

        public async Task<bool> IsImmediate(Guid releaseId, Guid releaseStatusId)
        {
            var releaseStatus = await GetAsync(releaseId, releaseStatusId);
            return releaseStatus.Immediate;
        }

        private Task<IEnumerable<ReleaseStatus>> ExecuteQueryAsync(TableQuery<ReleaseStatus> query)
        {
            return _tableStorageService.ExecuteQueryAsync(PublisherReleaseStatusTableName, query);
        }

        public async Task UpdateStateAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusState state)
        {
            await UpdateRowAsync(releaseId, releaseStatusId, row =>
            {
                row.State = state;
                return row;
            });
        }

        public async Task UpdateStagesAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusContentStage? content = null,
            ReleaseStatusDataStage? data = null, ReleaseStatusFilesStage? files = null,
            ReleaseStatusPublishingStage? publishing = null, ReleaseStatusOverallStage? overall = null,
            ReleaseStatusLogMessage logMessage = null)
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

            if (stage == ReleaseStatusDataStage.Failed)
            {
                await CancelReleasesWithContentDependency(releaseId, releaseStatusId);
            }
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
            return await _tableStorageService.GetTableAsync(PublisherReleaseStatusTableName);
        }

        /// <summary>
        /// Cancel publishing of any other releases sharing a dependency with the content of a failing release.
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
                publishing: ReleaseStatusPublishingStage.Scheduled);
            var scheduledExceptFailing = scheduled
                .Where(status => status.Id != failingReleaseStatusId);

            foreach (var releaseStatus in scheduledExceptFailing)
            {
                await UpdateStagesAsync(releaseStatus.ReleaseId, releaseStatus.Id,
                    publishing: ReleaseStatusPublishingStage.Cancelled, 
                    overall: ReleaseStatusOverallStage.Failed,
                    logMessage: new ReleaseStatusLogMessage(
                        $"Publishing cancelled due to dependency with failed Release: {failingReleaseId}, ReleaseStatusId: {failingReleaseStatusId}"));
            }
        }
    }
}