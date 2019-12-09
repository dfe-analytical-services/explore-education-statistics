using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseInfoStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Functions
{
    public class QueueReleaseFunction
    {
        private readonly IReleaseInfoService _releaseInfoService;
        private readonly IValidationService _validationService;

        public QueueReleaseFunction(IReleaseInfoService releaseInfoService, IValidationService validationService)
        {
            _releaseInfoService = releaseInfoService;
            _validationService = validationService;
        }

        public async Task QueueRelease(
            [QueueTrigger("releases")] QueueReleaseMessage message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().Name} function triggered: {message}");
            await ValidateReleaseAsync(message, async () => await AddReleaseInfo(message, Scheduled));
            logger.LogInformation($"{GetType().Name} function completed");
        }

        private async Task ValidateReleaseAsync(QueueReleaseMessage message, Func<Task> andThen)
        {
            var valid = await _validationService.ValidateAsync(message);
            await (valid ? andThen.Invoke() : AddReleaseInfo(message, FailedValidation));
        }

        private async Task AddReleaseInfo(QueueReleaseMessage message, ReleaseInfoStatus status)
        {
            await _releaseInfoService.AddReleaseInfoAsync(message, status);
        }
    }
}