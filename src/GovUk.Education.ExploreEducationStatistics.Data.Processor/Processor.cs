using System;
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
            try
            {
                logger.LogInformation($"{GetType().Name} function triggered: {message}");
                _importService.Import(message);
            }
            catch (Exception e)
            {
                // TODO Handle exceptions via notifications etc
                logger.LogError($"{GetType().Name} function FAILED: {e}");
                throw;
            }

            logger.LogInformation($"{GetType().Name} function completed");
        }
    }
}