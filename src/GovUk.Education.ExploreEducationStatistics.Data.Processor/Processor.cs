using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    // ReSharper disable once UnusedMember.Global
    public class Processor
    {
        private readonly IImportService _importService;

        public Processor(IImportService importService)
        {
            _importService = importService;
        }

        [FunctionName("Processor")]
        // ReSharper disable once UnusedMember.Global
        public void Run(
            [QueueTrigger("imports-pending", Connection = "")]
            ImportMessage message,
            ILogger logger)
        {
            logger.LogInformation($"{GetType().Name} function triggered: {message}");
            _importService.Import(message);
            logger.LogInformation($"{GetType().Name} function completed");
        }
    }
}