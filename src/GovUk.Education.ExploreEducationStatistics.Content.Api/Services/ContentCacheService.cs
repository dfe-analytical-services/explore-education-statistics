using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
using Microsoft.Azure.Storage.Blob;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services
{
    public class ContentCacheService : IContentCacheService
    {
        private readonly  CloudBlobClient _cloudBlobClient;
        private const string ContainerName = "cache";

        public ContentCacheService(CloudBlobClient cloudBlobClient)
        {
            _cloudBlobClient = cloudBlobClient;
        }
        
        public async Task<List<ThemeTree>> GetContentTreeAsync()
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"publications/tree.json");

            return !blob.Exists() ? null : JsonConvert.DeserializeObject<List<ThemeTree>>(blob.DownloadText());
        }
        
        public async Task<List<ThemeTree>> GetMethodologyTreeAsync()
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"methodology/tree.json");

            return !blob.Exists() ? null : JsonConvert.DeserializeObject<List<ThemeTree>>(blob.DownloadText());
        }
        
        public async Task<List<ThemeTree>> GetDownloadTreeAsync()
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"download/tree.json");

            return !blob.Exists() ? null : JsonConvert.DeserializeObject<List<ThemeTree>>(blob.DownloadText());
        }
        
        public async Task<Methodology> GetMethodologyAsync(string slug)
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"methodology/methodologies/{slug}.json");

            // TODO: this errors as the entire response is encoded as json and the converter will still try to convert the content blocks
            return !blob.Exists() ? null : JsonConvert.DeserializeObject<Methodology>(blob.DownloadText());
        }
        
        public async Task<PublicationViewModel> GetPublicationAsync(string slug)
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"publications/{slug}/publication.json");

            // TODO: this errors as the entire response is encoded as json and the converter will still try to convert the content blocks
            return !blob.Exists() ? null : JsonConvert.DeserializeObject<PublicationViewModel>(blob.DownloadText());
        }
        
        public async Task<ReleaseViewModel> GetLatestReleaseAsync(string slug)
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"publications/{slug}/latest-release.json");

            // TODO: this errors as the entire response is encoded as json and the converter will still try to convert the content blocks
            return !blob.Exists() ? null : JsonConvert.DeserializeObject<ReleaseViewModel>(blob.DownloadText());
        }
        
        public async Task<ReleaseViewModel> GetReleaseAsync(string publicationSlug, string releaseSlug)
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"publications/{publicationSlug}/releases/{releaseSlug}.json");

            // TODO: this errors as the entire response is encoded as json and the converter will still try to convert the content blocks
            return !blob.Exists() ? null : JsonConvert.DeserializeObject<ReleaseViewModel>(blob.DownloadText());
        }
        
        private async Task<CloudBlobContainer> GetCloudBlobContainer()
        {
            var blobContainer = _cloudBlobClient.GetContainerReference(ContainerName);
            await blobContainer.CreateIfNotExistsAsync();
            return blobContainer;
        }
    }
}