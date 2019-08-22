using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
        private readonly IReleaseService _releaseService;
        private readonly IPublicationService _publicationService;
        private readonly IDownloadService _downloadService;
        private readonly IMethodologyService _methodologyService;
        private readonly CloudBlobContainer _cloudBlobContainer;

        private const string ContainerName = "cache";

        public ContentCacheGenerationService(IDownloadService downloadService, IMethodologyService methodologyService,
            IContentService contentService, IReleaseService releaseService,IPublicationService publicationService, IConfiguration config)
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
            // Update the content trees
            await UpdateTrees();

            // TODO: Generate methodologies
            await UpdateMethodologies();
            
            // TODO: Generate releases
            // TODO: Work out how to identify latest release in the most efficient way

            // TODO: Generate publications?

            return true;
        }

        private async Task UpdateTrees()
        {
            await UpdateDownloadTree();
            await UpdateContentTree(); 
            await UpdateMethodologyTree();
        }

      private async Task UpdateContentTree()
        {
            var contentTree = _contentService.GetContentTree();

            if (contentTree != null)
            {
                var contentTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"content/tree.json");
                await contentTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(contentTree));
            }
            else
            {
                throw new Exception("Content tree could not be retrieved");
            }
        }

        private async Task UpdateDownloadTree()
        {
            // This is assuming the files have been copied first
            var downloadTree = _downloadService.GetDownloadTree();

            if (downloadTree != null)
            {
                var downloadTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"download/tree.json");
                await downloadTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(downloadTree));
            }
            else
            {
                throw new Exception("Download tree could not be retrieved");
            }
        }

        private async Task UpdateMethodologyTree()
        {
            var methodologyTree = _methodologyService.GetTree();

            if (methodologyTree != null)
            {
                var methodologyTreeBlob = _cloudBlobContainer.GetBlockBlobReference($"methodology/tree.json");
                await methodologyTreeBlob.UploadTextAsync(JsonConvert.SerializeObject(methodologyTree));
            }
            else
            {
                throw new Exception("Methodology tree could not be retrieved");
            }
        }
        
        
        private async Task UpdateMethodologies()
        {
            var methodologies = _methodologyService.Get();

            foreach (var methodology in methodologies)
            {
                // TODO: Save the filename as slug rather than ID
                var blob = _cloudBlobContainer.GetBlockBlobReference($"methodology/methodologies/{methodology}.json");
                await blob.UploadTextAsync(JsonConvert.SerializeObject(methodology));
            }
        }
        
        private async Task UpdatePublicationsAndReleases()
        {
            var publications = _publicationService.ListPublicationsWithPublishedReleases();

            foreach (var publication in publications)
            {
                // TODO: bit hacky but just to get the correct model for now
                var publicationViewModel = _publicationService.GetPublication(publication.Slug);
                
                // Save the publication
                var publicationBlob = _cloudBlobContainer.GetBlockBlobReference($"content/publications/{publication.Slug}.json");
                await publicationBlob.UploadTextAsync(JsonConvert.SerializeObject(publicationViewModel));
                
                
                // Save the Latest Release so that we can quickly retreive it
                var latestRelease = _releaseService.GetLatestRelease(publication.Slug);
                var latestReleaseBlob = _cloudBlobContainer.GetBlockBlobReference($"content/publications/{latestRelease.Publication.Slug}/releases/latest.json");
                await latestReleaseBlob.UploadTextAsync(JsonConvert.SerializeObject(latestRelease));


                // Save each release with its unique identifier
                foreach (var r in publication.Releases)
                {
                    var release = _releaseService.GetRelease(r.Slug);
                    
                    var releaseBlob = _cloudBlobContainer.GetBlockBlobReference($"content/releases/{release.Slug}.json");
                    await releaseBlob.UploadTextAsync(JsonConvert.SerializeObject(release));
                }
            }
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