using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusOverallStage;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusStates;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class NotifyChangeFunction
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IQueueService _queueService;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly IValidationService _validationService;

        public NotifyChangeFunction(IFileStorageService fileStorageService,
            IQueueService queueService,
            IReleasePublishingStatusService releasePublishingStatusService,
            IValidationService validationService)
        {
            _fileStorageService = fileStorageService;
            _queueService = queueService;
            _releasePublishingStatusService = releasePublishingStatusService;
            _validationService = validationService;
        }

        /// <summary>
        /// Azure function which validates that a Release is in a state to be published.
        /// Creates a ReleaseStatus entry scheduling its publication or triggers publishing files if immediate.
        /// </summary>
        /// <remarks>
        /// Validation will fail if the Release is already in the process of being published.
        /// A future schedule for publishing a Release that's not yet started will be cancelled.
        /// </remarks>
        /// <param name="message"></param>
        /// <param name="executionContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        [FunctionName("NotifyChange")]
        // ReSharper disable once UnusedMember.Global
        public async Task NotifyChange(
            [QueueTrigger(NotifyChangeQueue)] NotifyChangeMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation("{FunctionName} triggered: {Message}",
                executionContext.FunctionName,
                message);
            var lease = await _fileStorageService.AcquireLease(message.ReleaseId.ToString());
            try
            {
                await MarkScheduledReleaseStatusAsSuperseded(message);
                if (await _validationService.ValidatePublishingState(message.ReleaseId))
                {
                    await _validationService
                        .ValidateRelease(message.ReleaseId)
                        .OnSuccessDo(async () =>
                        {
                            if (message.Immediate)
                            {
                                var releaseStatus =
                                    await CreateReleaseStatusAsync(message, ImmediateReleaseStartedState);
                                await _queueService.QueuePublishReleaseFilesMessage(releaseStatus.ReleaseId,
                                    releaseStatus.Id);
                                await _queueService.QueuePublishReleaseContentMessage(message.ReleaseId,
                                    message.ReleaseStatusId);
                            }
                            else
                            {
                                // Create a Release Status entry here for the midnight job to pick up.
                                await CreateReleaseStatusAsync(message, ScheduledState);
                            }
                        })
                        .OnFailureDo(async logMessages =>
                        {
                            await CreateReleaseStatusAsync(message, InvalidState, logMessages);
                        });
                }
            }
            finally
            {
                await lease.Release();
            }

            logger.LogInformation("{FunctionName} completed", executionContext.FunctionName);
        }

        private async Task<ReleasePublishingStatus> CreateReleaseStatusAsync(NotifyChangeMessage message,
            ReleasePublishingStatusState state, IEnumerable<ReleasePublishingStatusLogMessage> logMessages = null)
        {
            return await _releasePublishingStatusService.CreateAsync(message.ReleaseId, message.ReleaseStatusId, state,
                message.Immediate, logMessages);
        }

        private async Task MarkScheduledReleaseStatusAsSuperseded(NotifyChangeMessage message)
        {
            // There may be an existing scheduled ReleaseStatus entry if this release has been validated before
            // If so, mark it as superseded
            var scheduled = await _releasePublishingStatusService.GetAllByOverallStage(message.ReleaseId, Scheduled);
            foreach (var releaseStatus in scheduled)
            {
                await _releasePublishingStatusService.UpdateStateAsync(message.ReleaseId, releaseStatus.Id, SupersededState);
            }
        }
    }
}
