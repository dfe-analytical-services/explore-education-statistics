using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentCacheGenerationService : IContentCacheGenerationService
    {
        private readonly IContentService _contentService;
        private readonly IDownloadService _downloadService;
        private readonly IMethodologyService _methodologyService;
        private readonly CloudBlobContainer _cloudBlobContainer;

        private const string ContainerName = "cache";

        public ContentCacheGenerationService(IDownloadService downloadService, IMethodologyService methodologyService,
            IContentService contentService, IConfiguration config)
        {
            _contentService = contentService;
            _downloadService = downloadService;
            _methodologyService = methodologyService;

            _cloudBlobContainer = GetCloudBlobContainer(config.GetConnectionString("PublicStorage"));
        }

        public async Task<bool> CleanAndRebuildFullCache()
        {
            await UpdateTrees();

            // TODO: Generate methodologies

            // TODO: Generate releases
            // TODO: Work out how to identify latest release in the most efficient way

            // TODO: Generate publications?

            return true;
        }

        private async Task UpdateTrees()
        {
            await UpdateDownloadTree();

            await UpdateMethodologyTree();

            await UpdateContentTree();
        }

        private async Task UpdateContentTree()
        {
            var contentTree = _contentService.GetContentTree();

            if (contentTree == null)
            {
                throw new Exception("Content tree could not be retrieved");
            }

            var downloadTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"content/tree.json");
            await downloadTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(contentTree));
        }

        private async Task UpdateDownloadTree()
        {
            var downloadTree = _downloadService.GetDownloadTree();

            if (downloadTree != null)
            {
                throw new Exception("Download tree could not be retrieved");
            }

            var downloadTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"download/tree.json");
            await downloadTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(downloadTree));
        }

        private async Task UpdateMethodologyTree()
        {
            var methodologyTree = _methodologyService.GetTree();
            
            if (methodologyTree != null)
            {
                throw new Exception("Methodology tree could not be retrieved");
            }

            var downloadTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"methodology/tree.json");
            await downloadTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(methodologyTree));
        }

        private static CloudBlobContainer GetCloudBlobContainer(string connectionString)
        {
            var cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var blobContainer = cloudBlobClient.GetContainerReference(ContainerName);
            blobContainer.CreateIfNotExists();
            return blobContainer;
        }
    }
}