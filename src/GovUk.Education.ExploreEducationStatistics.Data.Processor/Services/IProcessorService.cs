namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public interface IProcessorService
    {
        void ProcessFiles(
            ProcessorNotification processorNotification,
            string containerName,
            string blobStorageConnectionStr,
            string uploadsDir);
    }
}