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

        private static readonly (Stage Content, Stage Files, Stage Data, Stage Publishing, Stage Overall) InvalidStage =
            (Content: Cancelled, Files: Cancelled, Data: Cancelled, Publishing: Cancelled, Overall: Invalid);

        private static readonly (Stage Content, Stage Files, Stage Data, Stage Publishing, Stage Overall) ValidStage =
            (Content: Scheduled, Files: Scheduled, Data: Scheduled, Publishing: Scheduled, Overall: Scheduled);

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
                await CreateOrUpdateReleaseStatusAsync(statusMessage, ValidStage));
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task ValidateReleaseAsync(ReleaseStatusMessage statusMessage, Func<Task> andThen)
        {
            var (valid, logMessages) = await _validationService.ValidateAsync(statusMessage);
            await (valid
                ? andThen.Invoke()
                : CreateOrUpdateReleaseStatusAsync(statusMessage, InvalidStage, logMessages));
        }

        private async Task CreateOrUpdateReleaseStatusAsync(ReleaseStatusMessage statusMessage,
            (Stage, Stage, Stage, Stage, Stage) stage, IEnumerable<ReleaseStatusLogMessage> logMessages = null)
        {
            await _releaseStatusService.CreateOrUpdateAsync(statusMessage.ReleaseId, stage, logMessages);
        }
    }
}