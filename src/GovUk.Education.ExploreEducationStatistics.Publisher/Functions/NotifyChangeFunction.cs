using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
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
        private readonly IQueueService _queueService;
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly IValidationService _validationService;

        public NotifyChangeFunction(IQueueService queueService,
            IReleaseStatusService releaseStatusService,
            IValidationService validationService)
        {
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
            await MarkScheduledReleaseStatusAsSuperseded(message);
            await ValidateReleaseAsync(message, async () =>
            {
                if (message.Immediate)
                {
                    var releaseStatus = await CreateReleaseStatusAsync(message, ImmediateReleaseStartedState);
                    await _queueService.QueuePublishReleaseFilesMessageAsync(releaseStatus.ReleaseId, releaseStatus.Id);
                }
                else
                {
                    await CreateReleaseStatusAsync(message, ScheduledState);
                }
            });
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task ValidateReleaseAsync(NotifyChangeMessage message, Func<Task> andThen)
        {
            var (valid, logMessages) = 
                await _validationService.ValidateAsync(message.ReleaseId);
            await (valid ? andThen.Invoke() : CreateReleaseStatusAsync(message, InvalidState, logMessages));
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
    }
}