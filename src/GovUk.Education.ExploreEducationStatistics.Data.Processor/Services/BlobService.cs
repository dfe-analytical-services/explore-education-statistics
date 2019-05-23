using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class BlobService : IBlobService
    {
        public void MoveBlobBetweenContainers(CloudBlockBlob srcBlob, string destContainerName, string destFolder)
        {
            CloudBlockBlob destBlob;
            CloudBlobContainer destContainer = srcBlob.ServiceClient.GetContainerReference(destContainerName);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                srcBlob.DownloadToStreamAsync(memoryStream);

                destBlob = destContainer.GetBlockBlobReference(destFolder + "/" + Path.GetFileName(srcBlob.Uri.AbsolutePath));

                destBlob.StartCopyAsync(srcBlob);
            }

            srcBlob.DeleteAsync();
        }
    }
}
