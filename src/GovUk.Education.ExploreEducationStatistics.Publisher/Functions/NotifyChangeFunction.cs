using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusOverallStage;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusStates;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class NotifyChangeFunction
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IQueueService _queueService;
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly IValidationService _validationService;

        public NotifyChangeFunction(IFileStorageService fileStorageService,
            IQueueService queueService,
            IReleaseStatusService releaseStatusService,
            IValidationService validationService)
        {
            _fileStorageService = fileStorageService;
            _queueService = queueService;
            _releaseStatusService = releaseStatusService;
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
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            var lease = await _fileStorageService.AcquireLease(message.ReleaseId.ToString());
            try
            {
                await MarkScheduledReleaseStatusAsSuperseded(message);
                if (await _validationService.ValidatePublishingState(message.ReleaseId))
                {
                    await _validationService.ValidateRelease(message.ReleaseId)
                        .OnSuccessDo(async () =>
                        {
                            if (message.Immediate)
                            {
                                var releaseStatus =
                                    await CreateReleaseStatusAsync(message, ImmediateReleaseStartedState);
                                await _queueService.QueuePublishReleaseFilesMessageAsync(releaseStatus.ReleaseId,
                                    releaseStatus.Id);
                            }
                            else
                            {
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
                ReleaseLease(lease);
            }

            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task<ReleaseStatus> CreateReleaseStatusAsync(NotifyChangeMessage message,
            ReleaseStatusState state, IEnumerable<ReleaseStatusLogMessage> logMessages = null)
        {
            return await _releaseStatusService.CreateAsync(message.ReleaseId, state, message.Immediate, logMessages);
        }

        private async Task MarkScheduledReleaseStatusAsSuperseded(NotifyChangeMessage message)
        {
            // There may be an existing scheduled ReleaseStatus entry if this release has been validated before
            // If so, mark it as superseded
            var scheduled = await _releaseStatusService.GetAllByOverallStage(message.ReleaseId, Scheduled);
            foreach (var releaseStatus in scheduled)
            {
                await _releaseStatusService.UpdateStateAsync(message.ReleaseId, releaseStatus.Id, SupersededState);
            }
        }
        
        private static void ReleaseLease((CloudBlockBlob blob, string id) lease)
        {
            lease.blob.ReleaseLease(AccessCondition.GenerateLeaseCondition(lease.id));
        }
    }
}