namespace GovUK.Education.ExploreEducationStatistics.Data.Processor.Services
{
    using Microsoft.WindowsAzure.Storage.Blob;

    public interface IBlobService
    {
        void MoveBlobBetweenContainers(CloudBlockBlob srcBlob, string destContainerName, string destFolder);
    }
}
