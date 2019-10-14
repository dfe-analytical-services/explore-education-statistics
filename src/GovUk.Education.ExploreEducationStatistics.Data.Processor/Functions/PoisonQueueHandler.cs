using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
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
            var errors = new List<string>() { "File failed to import for unknown reason in upload processing stage."};
            
            await _batchService.FailImport(
                message.Release.Id.ToString(),
                message.DataFileName,
                errors
            );
        }


        [FunctionName("ImportObservationsPoisonHandler")]
        public async void ImportObservationsPoisonQueueHandler(
            [QueueTrigger("imports-available-poison")]
            ImportMessage message,
            ILogger logger)
        {
            var errors = new List<string>() { "File failed to import for unknown reason in observation import stage." };
            
            await _batchService.FailImport(
                message.Release.Id.ToString(),
                // N.B. need real file name - potentially this could be a batched file name so convert
                ProcessorUtils.GetDataFileName(message),
                errors
            );
        }
    }
}