using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storageConnectionString;

        public FileStorageService(IConfiguration config)
        {
            _storageConnectionString = config.GetConnectionString("AzureStorage");
        }

        public async Task CopyFilesAsync(string publication, string release, string sourceContainerName, string destContainerName)
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var sourceContainer = blobClient.GetContainerReference(sourceContainerName);
            var destinationContainer = blobClient.GetContainerReference(destContainerName);

            var sourceBlob = sourceContainer.GetBlockBlobReference($"{publication}/{release}");
            var targetBlob = destinationContainer.GetBlockBlobReference($"{publication}/{release}");

            await targetBlob.StartCopyAsync(sourceBlob);
        }
    }
}