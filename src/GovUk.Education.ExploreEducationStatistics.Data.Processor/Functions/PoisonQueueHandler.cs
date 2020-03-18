using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Functions
{
    public class PoisonQueueHandler
    {
        private readonly IBatchService _batchService;

        public PoisonQueueHandler(IBatchService batchService)
        {
            _batchService = batchService;
        }

        [FunctionName("ProcessUploadsPoisonHandler")]
        public async void ProcessUploadsPoisonQueueHandler(
            [QueueTrigger("imports-pending-poison")]
            ImportMessage message,
            ILogger logger
        )
        {
            await _batchService.FailImport(
                message.Release.Id.ToString(),
                message.OrigDataFileName,
                new List<ValidationError>
                {
                    new ValidationError("File failed to import for unknown reason in upload processing stage.")
                }.AsEnumerable()
            );
        }

        [FunctionName("ImportObservationsPoisonHandler")]
        public async void ImportObservationsPoisonQueueHandler(
            [QueueTrigger("imports-available-poison")]
            ImportMessage message,
            ILogger logger)
        {
            await _batchService.FailImport(
                message.Release.Id.ToString(),
                message.OrigDataFileName,
                new List<ValidationError>
                {
                    new ValidationError("File failed to import for unknown reason in upload processing stage.")
                }.AsEnumerable()
            );
        }
    }
}