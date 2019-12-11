using System;
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
            [QueueTrigger("releases")] ValidateReleaseMessage message,
            ExecutionContext executionContext,
            ILogger logger)
        {
            logger.LogInformation($"{executionContext.FunctionName} triggered: {message}");
            await ValidateReleaseAsync(message, async () => await AddReleaseStatus(message, Scheduled));
            logger.LogInformation($"{executionContext.FunctionName} completed");
        }

        private async Task ValidateReleaseAsync(ValidateReleaseMessage message, Func<Task> andThen)
        {
            var valid = await _validationService.ValidateAsync(message);
            await (valid ? andThen.Invoke() : AddReleaseStatus(message, Invalid));
        }

        private async Task AddReleaseStatus(ValidateReleaseMessage message, Stage stage)
        {
            await _releaseStatusService.AddAsync(message.PublicationSlug,
                message.PublishScheduled,
                message.ReleaseId,
                message.ReleaseSlug,
                stage);
        }
    }
}