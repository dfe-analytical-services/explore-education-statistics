using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
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
        }

        [FunctionName("ProcessUploadsPoisonHandler")]
        public void ProcessUploadsPoisonQueueHandler(
            [QueueTrigger("imports-pending-poison")]
            ImportMessage message,
            ILogger logger
        )
        {
            var errors = new List<string>() { "File failed to import for unknown reason in upload processing stage."};
            
            _batchService.FailImport(
                message.Release.Id.ToString(),
                message.DataFileName,
                errors
            ).Wait();
        }


        [FunctionName("ImportObservationsPoisonHandler")]
        public void ImportObservationsPoisonQueueHandler(
            [QueueTrigger("imports-available-poison")]
            ImportMessage message,
            ILogger logger)
        {
            var errors = new List<string>() { "File failed to import for unknown reason in observation import stage." };
            
            _batchService.FailImport(
                message.Release.Id.ToString(),
                message.DataFileName,
                errors
            ).Wait();
        }
    }
}