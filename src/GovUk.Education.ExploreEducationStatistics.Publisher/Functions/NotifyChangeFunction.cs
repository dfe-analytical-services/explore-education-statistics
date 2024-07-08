using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class NotifyChangeFunction(
        ILogger<NotifyChangeFunction> logger,
        IFileStorageService fileStorageService,
        IQueueService queueService,
        IReleasePublishingStatusService releasePublishingStatusService,
        IValidationService validationService)
    {
        /// <summary>
        /// Azure function which validates that a release version is in a state to be published.
        /// Creates a ReleaseStatus entry scheduling its publication or triggers publishing files if immediate.
        /// </summary>
        /// <remarks>
        /// Validation will fail if the release version is already in the process of being published.
        /// A future schedule for publishing a release version that's not yet started will be cancelled.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [Function("NotifyChange")]
        public async Task NotifyChange(
            [QueueTrigger(NotifyChangeQueue)] NotifyChangeMessage message,
            FunctionContext context)
        {
            logger.LogInformation("{FunctionName} triggered: {Message}",
                context.FunctionDefinition.Name,
                message);
            var lease = await fileStorageService.AcquireLease(message.ReleaseVersionId.ToString());
            try
            {
                await MarkScheduledReleaseStatusAsSuperseded(message);
                if (await validationService.ValidatePublishingState(message.ReleaseVersionId))
                {
                    await validationService
                        .ValidateRelease(message.ReleaseVersionId)
                        .OnSuccessDo(async () =>
                        {
                            if (message.Immediate)
                            {
                                var releasePublishingKey = await CreateReleaseStatus(message,
                                    ReleasePublishingStatusStates.ImmediateReleaseStartedState);
                                await queueService.QueuePublishReleaseFilesMessages(new[] { releasePublishingKey });
                                await queueService.QueuePublishReleaseContentMessage(releasePublishingKey);
                            }
                            else
                            {
                                // Create a Release Status entry here for the midnight job to pick up.
                                await CreateReleaseStatus(message, ReleasePublishingStatusStates.ScheduledState);
                            }
                        })
                        .OnFailureDo(async logMessages => await CreateReleaseStatus(message,
                            ReleasePublishingStatusStates.InvalidState,
                            logMessages));
                }
            }
            finally
            {
                await lease.Release();
            }

            logger.LogInformation("{FunctionName} completed", context.FunctionDefinition.Name);
        }

        private async Task<ReleasePublishingKey> CreateReleaseStatus(
            NotifyChangeMessage message,
            ReleasePublishingStatusState state,
            IEnumerable<ReleasePublishingStatusLogMessage>? logMessages = null)
        {
            return await releasePublishingStatusService.Create(
                message.ReleasePublishingKey,
                state,
                message.Immediate,
                logMessages);
        }

        private async Task MarkScheduledReleaseStatusAsSuperseded(NotifyChangeMessage message)
        {
            // There may be an existing scheduled ReleaseStatus entry if this release version has been validated before
            // If so, mark it as superseded
            var scheduled = await releasePublishingStatusService.GetAllByOverallStage(
                message.ReleaseVersionId,
                ReleasePublishingStatusOverallStage.Scheduled);

            await scheduled
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async status =>
                    await releasePublishingStatusService.UpdateState(status.AsTableRowKey(),
                        ReleasePublishingStatusStates.SupersededState));
        }
    }
}
