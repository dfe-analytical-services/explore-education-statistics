using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class BlobService : IBlobService
    {
        public async Task MoveBlobBetweenContainers(CloudBlockBlob srcBlob, string destContainerName, string destFolder)
        {
            CloudBlockBlob destBlob;
            CloudBlobContainer destContainer = srcBlob.ServiceClient.GetContainerReference(destContainerName);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                await srcBlob.DownloadToStreamAsync(memoryStream);

                destBlob = destContainer.GetBlockBlobReference(destFolder + "/" + Path.GetFileName(srcBlob.Uri.AbsolutePath));

                await destBlob.StartCopyAsync(srcBlob);
            }

            srcBlob.DeleteAsync();
        }
    }
}
