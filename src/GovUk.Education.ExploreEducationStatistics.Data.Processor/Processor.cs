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
        private readonly IFileImportService _fileImportService;
        private readonly IValidationService _validationService;

        public Processor(
            IFileImportService fileImportService,
            IValidationService validationService
        )
        {
            _fileImportService = fileImportService;
            _validationService = validationService;
        }

        [FunctionName("ProcessUploads")]
        // ReSharper disable once UnusedMember.Global
        public void ProcessUploads(
            [QueueTrigger("imports-pending", Connection = "")]
            ImportMessage message,
            ILogger logger,
            [Queue("imports-available")] ICollector<ImportMessage> collector
            )
        {
            try
            {
                logger.LogInformation($"{GetType().Name} function triggered: {message}");
                _validationService.Validate(collector, message);
            }
            catch (Exception e)
            {
                // TODO Handle exceptions via notifications etc
                logger.LogError($"{GetType().Name} function FAILED: {e}");
                throw;
            }

            logger.LogInformation($"{GetType().Name} function completed");
        }
        
        [FunctionName("ImportFiles")]
        // ReSharper disable once UnusedMember.Global
        public void ImportFiles(
            [QueueTrigger("imports-available", Connection = "")]
            ImportMessage message,
            ILogger logger)
        {
            try
            {
                logger.LogInformation($"{GetType().Name} function triggered: {message}");
                _fileImportService.ImportFiles(message);
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