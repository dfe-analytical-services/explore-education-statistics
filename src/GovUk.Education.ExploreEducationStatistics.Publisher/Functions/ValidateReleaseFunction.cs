using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class ValidateReleaseFunction
    {
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly IValidationService _validationService;

        private static readonly ReleaseStatusState InvalidState =
            new ReleaseStatusState(Cancelled, Cancelled, Cancelled, Cancelled, Invalid);

        private static readonly ReleaseStatusState ValidState =
            new ReleaseStatusState(NotStarted, NotStarted, NotStarted, NotStarted, Scheduled);

        public ValidateReleaseFunction(IReleaseStatusService releaseStatusService, IValidationService validationService)
        {
            _releaseStatusService = releaseStatusService;
            _validationService = validationService;
        }

        /**
         * Azure function which validates that a Release is in a state to be published and if so creates a ReleaseStatus entry scheduling its publication.
         */
        [FunctionName("ValidateRelease")]
        public async Task ValidateRelease(
            [QueueTrigger("releases")] ReleaseStatusMessage statusMessage,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {statusMessage}");
            await ValidateReleaseAsync(statusMessage, async () =>
                await CreateOrUpdateReleaseStatusAsync(statusMessage, ValidState));
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task ValidateReleaseAsync(ReleaseStatusMessage statusMessage, Func<Task> andThen)
        {
            var (valid, logMessages) = await _validationService.ValidateAsync(statusMessage);
            await (valid
                ? andThen.Invoke()
                : CreateOrUpdateReleaseStatusAsync(statusMessage, InvalidState, logMessages));
        }

        private async Task CreateOrUpdateReleaseStatusAsync(ReleaseStatusMessage statusMessage,
            ReleaseStatusState state, IEnumerable<ReleaseStatusLogMessage> logMessages = null)
        {
            await _releaseStatusService.CreateOrUpdateAsync(statusMessage.ReleaseId, state, logMessages);
        }
    }
}