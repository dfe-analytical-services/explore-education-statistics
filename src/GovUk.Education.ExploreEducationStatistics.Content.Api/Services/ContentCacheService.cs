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
        private readonly CloudBlobClient _cloudBlobClient;
        private const string ContainerName = "cache";

        public ContentCacheService(CloudBlobClient cloudBlobClient)
        {
            _cloudBlobClient = cloudBlobClient;
        }

        public async Task<string> GetContentTreeAsync()
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference("publications/tree.json");

            return !blob.Exists() ? null :  await blob.DownloadTextAsync();
        }

        public async Task<string> GetMethodologyTreeAsync()
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference("methodology/tree.json");

            return !blob.Exists() ? null :  await blob.DownloadTextAsync();
        }

        public async Task<string> GetDownloadTreeAsync()
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference("download/tree.json");

            return !blob.Exists() ? null :  await blob.DownloadTextAsync();
        }

        public async Task<string> GetMethodologyAsync(string slug)
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"methodology/methodologies/{slug}.json");

            return !blob.Exists() ? null :  await blob.DownloadTextAsync();
        }

        public async Task<string> GetPublicationAsync(string slug)
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"publications/{slug}/publication.json");

            return !blob.Exists() ? null :  await blob.DownloadTextAsync();
        }

        public async Task<string> GetLatestReleaseAsync(string slug)
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"publications/{slug}/latest-release.json");

            return !blob.Exists() ? null :  await blob.DownloadTextAsync();
        }

        public async Task<string> GetReleaseAsync(string publicationSlug, string releaseSlug)
        {
            var container = await GetCloudBlobContainer();

            var blob = container.GetBlockBlobReference($"publications/{publicationSlug}/releases/{releaseSlug}.json");

            return !blob.Exists() ? null :  await blob.DownloadTextAsync();
        }

        private async Task<CloudBlobContainer> GetCloudBlobContainer()
        {
            var blobContainer = _cloudBlobClient.GetContainerReference(ContainerName);
            await blobContainer.CreateIfNotExistsAsync();
            return blobContainer;
        }
    }
}