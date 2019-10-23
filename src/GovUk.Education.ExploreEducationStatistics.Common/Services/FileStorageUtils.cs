using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class FileStorageUtils
    {
        public static async Task<CloudBlobContainer> GetCloudBlobContainerAsync(string storageConnectionString,
            string containerName, BlobContainerPermissions permissions = null)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            await blobContainer.CreateIfNotExistsAsync();

            if (permissions != null)
            {
                await blobContainer.SetPermissionsAsync(permissions);
            }

            return blobContainer;
        }

        public static CloudBlobContainer GetCloudBlobContainer(string storageConnectionString, string containerName,
            BlobContainerPermissions permissions = null)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExists();

            if (permissions != null)
            {
                blobContainer.SetPermissions(permissions);
            }

            return blobContainer;
        }
    }
}