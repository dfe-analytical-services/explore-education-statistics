namespace GovUK.Education.ExploreEducationStatistics.Data.Processor.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IProcessorService
    {
        void ProcessFiles(
            ProcessorNotification processorNotification,
            string containerName,
            string blobStorageConnectionStr,
            string uploadsDir);
    }
}
