using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using static GovUk.Education.ExploreEducationStatistics.Data.Processor.Model.ImporterQueues;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    public class PoisonQueueHandler
    {
        private readonly IDataImportService _dataImportService;

        public PoisonQueueHandler(IDataImportService dataImportService)
        {
            _dataImportService = dataImportService;
        }

        [FunctionName("ProcessUploadsPoisonHandler")]
        public async Task ProcessUploadsPoisonQueueHandler(
            [QueueTrigger(ImportsPendingPoisonQueue)]
            ImportMessage message)
        {
            await _dataImportService.FailImport(message.Id,
                "File failed to import for unknown reason in upload processing stage.");
        }

        [FunctionName("ImportObservationsPoisonHandler")]
        public async Task ImportObservationsPoisonQueueHandler(
            [QueueTrigger(ImportsAvailablePoisonQueue)]
            ImportMessage message)
        {
            await _dataImportService.FailImport(message.Id,
                "File failed to import for unknown reason in upload processing stage.");
        }
    }
}