using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentCacheGenerationService : IContentCacheGenerationService
    {
        private readonly IDownloadService _downloadService;
        private readonly CloudBlobContainer _cloudBlobContainer;
        
        private const string ContainerName = "cache";

        public ContentCacheGenerationService(IDownloadService downloadService, CloudBlobClient cloudBlobClient)
        {
            _downloadService = downloadService;
            
            _cloudBlobContainer = GetCloudBlobContainer(cloudBlobClient);
        }

        public async Task<bool> CleanAndRebuildFullCache()
        {
            // download tree
            await UpdateDownloadTree();

            // content tree

            // methodology tree


            // methodologies


            // publications & releases



            return true;
        }

        private async Task UpdateDownloadTree()
        {
            var downloadTree = _downloadService.GetDownloadTree();

            var downloadTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"download/tree.json");
            await downloadTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(downloadTree));
        }

        private CloudBlobContainer GetCloudBlobContainer(CloudBlobClient cloudBlobClient)
        {
            var blobContainer = cloudBlobClient.GetContainerReference(ContainerName);
            blobContainer.CreateIfNotExists();
            return blobContainer;
        }
    }
}