using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public interface IBlobService
    {
        void MoveBlobBetweenContainers(CloudBlockBlob srcBlob, string destContainerName, string destFolder);
    }
}
