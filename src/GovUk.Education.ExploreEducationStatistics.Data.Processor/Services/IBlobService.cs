using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public interface IBlobService
    {
        Task MoveBlobBetweenContainers(CloudBlockBlob srcBlob, string destContainerName, string destFolder);
    }
}