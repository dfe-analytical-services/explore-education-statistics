using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    // ReSharper disable once UnusedType.Global
    public class ValidateReleaseFunction
    {
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly IValidationService _validationService;

        private static readonly ReleaseStatusState InvalidState =
            new ReleaseStatusState(ReleaseStatusContentStage.Cancelled,
                ReleaseStatusFilesStage.Cancelled,
                ReleaseStatusDataStage.Cancelled,
                ReleaseStatusPublishingStage.Cancelled,
                ReleaseStatusOverallStage.Invalid);

        private static readonly ReleaseStatusState ValidState = 
            new ReleaseStatusState(ReleaseStatusContentStage.NotStarted,
                ReleaseStatusFilesStage.NotStarted,
                ReleaseStatusDataStage.NotStarted,
                ReleaseStatusPublishingStage.NotStarted,
                ReleaseStatusOverallStage.Scheduled);

        public ValidateReleaseFunction(IReleaseStatusService releaseStatusService, IValidationService validationService)
        {
            _releaseStatusService = releaseStatusService;
            _validationService = validationService;
        }

        /**
         * Azure function which validates that a Release is in a state to be published and if so creates a ReleaseStatus entry scheduling its publication.
         */
        [FunctionName("ValidateRelease")]
        // ReSharper disable once UnusedMember.Global
        public async Task ValidateRelease(
            [QueueTrigger("releases")] ReleaseStatusMessage statusMessage,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {statusMessage}");
            await ValidateReleaseAsync(statusMessage, async () =>
                // TODO EESB-28 fail is there is already a run that is Started
                // TODO EESB-28 cancel an existing run that is already Scheduled
                await CreateReleaseStatusAsync(statusMessage, ValidState));
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task ValidateReleaseAsync(ReleaseStatusMessage statusMessage, Func<Task> andThen)
        {
            var (valid, logMessages) = await _validationService.ValidateAsync(statusMessage);
            await (valid ? andThen.Invoke() : CreateReleaseStatusAsync(statusMessage, InvalidState, logMessages));
        }

        private async Task CreateReleaseStatusAsync(ReleaseStatusMessage statusMessage,
            ReleaseStatusState state, IEnumerable<ReleaseStatusLogMessage> logMessages = null)
        {
            await _releaseStatusService.CreateAsync(statusMessage.ReleaseId, state, logMessages);
        }
    }
}