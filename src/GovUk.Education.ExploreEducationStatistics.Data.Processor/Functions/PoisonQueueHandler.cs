using System;
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
        public PoisonQueueHandler()
        {
        }

        [FunctionName("ProcessUploadsPoisonHandler")]
        public void ProcessUploads(
            [QueueTrigger("imports-pending-poison")]
            ImportMessage message,
            ILogger logger
        )
        {
            // TODO: Flag these files as failed
        }


        [FunctionName("ImportObservationsPoisonHandler")]
        public void ImportObservations(
            [QueueTrigger("imports-available-poison")]
            ImportMessage message,
            ILogger logger)
        {
            // TODO: Flag these files as failed
        }
    }
}