using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    public class PoisonQueueHandler
    {
        private readonly IDataImportService _dataImportService;
        private readonly ILogger<PoisonQueueHandler> _logger;

        public PoisonQueueHandler(
            IDataImportService dataImportService, 
            ILogger<PoisonQueueHandler> logger)
        {
            _dataImportService = dataImportService;
            _logger = logger;
        }

        [FunctionName("ProcessUploadsPoisonHandler")]
        public async Task ProcessUploadsPoisonQueueHandler(
            [QueueTrigger(ImportsPendingPoisonQueue)]
            ImportMessage message)
        {
            try
            {
                await _dataImportService.FailImport(message.Id,
                    "File failed to import for unknown reason in upload processing stage.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception caught whilst processing ProcessUploadsPoisonQueueHandler function: {ex.StackTrace}");
            }
        }

        [FunctionName("ImportObservationsPoisonHandler")]
        public async Task ImportObservationsPoisonQueueHandler(
            [QueueTrigger(ImportsAvailablePoisonQueue)]
            ImportMessage message)
        {
            try
            {
                await _dataImportService.FailImport(message.Id,
                    "File failed to import for unknown reason in upload processing stage.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception caught whilst processing ImportObservationsPoisonHandler function: {ex.StackTrace}");
            }
        }
    }
}