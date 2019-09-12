using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.ViewModels;
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
        private readonly IReleaseService _releaseService;
        private readonly IPublicationService _publicationService;
        private readonly IDownloadService _downloadService;
        private readonly IMethodologyService _methodologyService;
        private readonly CloudBlobContainer _cloudBlobContainer;

        private const string ContainerName = "cache";

        public ContentCacheGenerationService(IDownloadService downloadService, IMethodologyService methodologyService,
            IContentService contentService, IReleaseService releaseService, IPublicationService publicationService,
            IConfiguration config)
        {
            _contentService = contentService;
            _releaseService = releaseService;
            _publicationService = publicationService;
            _downloadService = downloadService;
            _methodologyService = methodologyService;

            _cloudBlobContainer = GetCloudBlobContainer(config.GetConnectionString("PublicStorage"));
        }

        public async Task<bool> CleanAndRebuildFullCache()
        {
            try
            {
                var trees = await UpdateTrees();

                var publications = await UpdatePublicationsAndReleases();

                var methodologies = await UpdateMethodologies();

                return trees && methodologies && publications;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        private async Task<bool> UpdateTrees()
        {
            var downloadTree = await UpdateDownloadTree();
            var contentTree = await UpdateContentTree();
            var methodologyTree = await UpdateMethodologyTree();

            return downloadTree && contentTree && methodologyTree;
        }

        private async Task<bool> UpdateContentTree()
        {
            var contentTree = _contentService.GetContentTree();

            if (contentTree != null)
            {
                var contentTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"publications/tree.json");
                await contentTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(contentTree, null,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    }));
                return true;
            }
            else
            {
                throw new Exception("Content tree could not be retrieved");
            }
        }

        // TODO: Get this to work
        private async Task<bool> UpdateDownloadTree()
        {
            // This is assuming the files have been copied first
            var downloadTree = _downloadService.GetDownloadTree();

            if (downloadTree != null)
            {
                var downloadTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"download/tree.json");
                await downloadTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(downloadTree, null,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    }));
                return true;
            }
            else
            {
                throw new Exception("Download tree could not be retrieved");
            }
        }

        private async Task<bool> UpdateMethodologyTree()
        {
            var methodologyTree = _methodologyService.GetTree();

            var methodologyTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"methodology/tree.json");
            await methodologyTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(methodologyTree, null,
                new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                }));
            return true;
        }


        private async Task<bool> UpdateMethodologies()
        {
            var methodologies = _methodologyService.Get();

            foreach (var methodology in methodologies)
            {
                // TODO: model might be incorrect so will need to validate this
                // TODO: Save the filename as slug rather than ID
                var blob = _cloudBlobContainer.GetBlockBlobReference(
                    $"methodology/methodologies/{methodology.Slug}.json");
                await blob.UploadTextAsync(JsonConvert.SerializeObject(methodology, null,
                    new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));
            }

            return true;
        }

        private async Task<bool> UpdatePublicationsAndReleases()
        {
            var publications = _publicationService.ListPublicationsWithPublishedReleases();

            foreach (var publication in publications)
            {
                var publicationBlob =
                    _cloudBlobContainer.GetBlockBlobReference($"publications/{publication.Slug}/publication.json");
                await publicationBlob.UploadTextAsync(JsonConvert.SerializeObject(
                    new PublicationViewModel() {Id = publication.Id, Title = publication.Title}, null,
                    new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));

                var latestRelease = _releaseService.GetLatestRelease(publication.Id);
                var latestReleaseJson = JsonConvert.SerializeObject(latestRelease, null,
                    new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});

                var latestReleaseBlob =
                    _cloudBlobContainer.GetBlockBlobReference(
                        $"publications/{latestRelease.Publication.Slug}/latest-release.json");
                var latestReleaseYearBlob = _cloudBlobContainer.GetBlockBlobReference(
                    $"publications/{latestRelease.Publication.Slug}/releases/{latestRelease.Slug}.json");

                await latestReleaseBlob.UploadTextAsync(latestReleaseJson);
                await latestReleaseYearBlob.UploadTextAsync(latestReleaseJson);

                foreach (var r in publication.Releases)
                {
                    var release = _releaseService.GetRelease(r.Id);

                    var releaseBlob =
                        _cloudBlobContainer.GetBlockBlobReference(
                            $"publications/{publication.Slug}/releases/{release.Slug}.json");

                    // TODO: Fix model to ignore self referencing loop (quick workaround)
                    var json = JsonConvert.SerializeObject(release, null,
                        new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});

                    await releaseBlob.UploadTextAsync(json);
                }
            }

            return true;
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