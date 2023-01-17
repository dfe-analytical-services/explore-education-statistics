#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusStates;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class StageScheduledReleasesFunction
    {
        private readonly IQueueService _queueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;

        public StageScheduledReleasesFunction(IQueueService queueService, IReleasePublishingStatusService releasePublishingStatusService)
        {
            _queueService = queueService;
            _releasePublishingStatusService = releasePublishingStatusService;
        }

        /// <summary>
        /// Azure function which triggers publishing files and staging content for all Releases that are scheduled to
        /// be published later during the day. This operates on a schedule which by default occurs at midnight every
        /// night.
        /// </summary>
        /// <param name="timer"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        [FunctionName("StageScheduledReleases")]
        // ReSharper disable once UnusedMember.Global
        public async Task StageScheduledReleases([TimerTrigger("%PublishReleasesCronSchedule%")]
            TimerInfo timer,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                executionContext.FunctionName,
                DateTime.UtcNow);

            await PublishReleaseFilesAndStageContent();

            logger.LogInformation(
                "{FunctionName} completed.  Will be scheduled again to run at {NextDateTime}",
                executionContext.FunctionName,
                timer.FormatNextOccurrences(1));
        }

        /// <summary>
        /// Azure function which triggers publishing files and staging content for all Releases that are scheduled to
        /// be published later during the day. This is triggered manually by an HTTP post request, and is disabled in
        /// production environments.
        /// </summary>
        /// <param name="request">
        /// An optional JSON request body with a "ReleaseIds" array can be included in the POST request to limit the
        /// scope of the Function to only the provided Release Ids.
        /// </param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        [FunctionName("StageScheduledReleasesImmediately")]
        // ReSharper disable once UnusedMember.Global
        public async Task StageScheduledReleasesImmediately(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequest request,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered at: {DateTime}",
                executionContext.FunctionName,
                DateTime.UtcNow);       

            var releaseIds = (await request.GetJsonBody<ManualTriggerRequest>())?.ReleaseIds;
            
            var processedReleaseIds = await PublishReleaseFilesAndStageContent(releaseIds);

            logger.LogInformation("{FunctionName} completed - staged Releases {ReleaseIds}", 
                executionContext.FunctionName,
                processedReleaseIds.JoinToString(','));
        }

        private async Task<Guid[]> PublishReleaseFilesAndStageContent(Guid[]? specificReleaseIds = null)
        {
            var scheduled = (await QueryScheduledReleases())
                .Select(status => (status.ReleaseId, status.Id))
                .ToList();

            var toProcess = specificReleaseIds.IsNullOrEmpty() 
                ? scheduled 
                : scheduled
                    .Where(releaseStatus => specificReleaseIds!.Contains(releaseStatus.ReleaseId))
                    .ToList();

            if (!toProcess.Any())
            {
                return Array.Empty<Guid>();
            }

            await toProcess
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(ids =>
                {
                    var (releaseId, releaseStatusId) = ids;
                    return _releasePublishingStatusService
                        .UpdateStateAsync(releaseId, releaseStatusId, ScheduledReleaseStartedState);
                });

            await _queueService.QueuePublishReleaseFilesMessage(toProcess);
            await _queueService.QueueGenerateStagedReleaseContentMessage(toProcess);
            
            return toProcess
                .Select(releaseStatus => releaseStatus.ReleaseId)
                .ToArray();
        }

        private async Task<IEnumerable<ReleasePublishingStatus>> QueryScheduledReleases()
        {
            return await _releasePublishingStatusService.GetWherePublishingDueTodayWithStages(
                overall: ReleasePublishingStatusOverallStage.Scheduled);
        }
        
        // ReSharper disable once ClassNeverInstantiated.Local
        private record ManualTriggerRequest(Guid[] ReleaseIds);
    }
}
