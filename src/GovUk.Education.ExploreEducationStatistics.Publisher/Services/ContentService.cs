using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentService : IContentService
    {
        private readonly IFastTrackService _fastTrackService;
        private readonly IReleaseService _releaseService;
        private readonly IPublicationService _publicationService;
        private readonly IDownloadService _downloadService;
        private readonly IMethodologyService _methodologyService;
        private readonly IFileStorageService _fileStorageService;

        private readonly JsonSerializerSettings _jsonSerializerSettingsCamelCase =
            GetJsonSerializerSettings(new CamelCaseNamingStrategy());

        public ContentService(IFastTrackService fastTrackService,
            IDownloadService downloadService,
            IFileStorageService fileStorageService,
            IMethodologyService methodologyService,
            IReleaseService releaseService,
            IPublicationService publicationService)
        {
            _fastTrackService = fastTrackService;
            _fileStorageService = fileStorageService;
            _releaseService = releaseService;
            _publicationService = publicationService;
            _downloadService = downloadService;
            _methodologyService = methodologyService;
        }

        public async Task DeletePreviousVersionsContent(params Guid[] releaseIds)
        {
            var releases = await _releaseService.GetAmendedReleases(releaseIds);

            foreach (var release in releases)
            {
                await _fastTrackService.DeleteAllFastTracksByRelease(release.PreviousVersionId);

                // Delete content which hasn't been overwritten because the Slug has changed
                if (release.Slug != release.PreviousVersion.Slug)
                {
                    await _fileStorageService.DeletePublicBlob(
                        PublicContentReleasePath(release.Publication.Slug, release.PreviousVersion.Slug));
                }
            }
        }
        
        public async Task DeletePreviousVersionsDownloadFiles(params Guid[] releaseIds)
        {
            var releases = await _releaseService.GetAsync(releaseIds);

            foreach (var release in releases)
            {
                await _fileStorageService.DeleteDownloadFilesForPreviousVersion(release);
            }
        }

        /**
         * Intended to be used as a Development / BAU Function to perform a full content refresh
         */
        public async Task UpdateAllContentAsync()
        {
            var context = new PublishContext(DateTime.UtcNow, false);

            await DeleteAllContent();
            await CacheTrees(context);

            var publications = _publicationService.ListPublicationsWithPublishedReleases().ToList();
            var methodologyIds = publications.Where(publication => publication.MethodologyId.HasValue)
                .Select(publication => publication.MethodologyId.Value).Distinct();

            foreach (var publication in publications)
            {
                var releases = publication.Releases.Where(release => release.Live);
                await CachePublication(publication.Id, context);
                await CacheLatestRelease(publication, context);
                foreach (var release in releases)
                {
                    await CacheFastTracks(release, context);
                    await CacheRelease(release, context);
                }
            }

            foreach (var methodologyId in methodologyIds)
            {
                await CacheMethodology(methodologyId, context);
            }
        }

        public async Task UpdateContent(PublishContext context, params Guid[] releaseIds)
        {
            await CacheTrees(context, releaseIds);

            var releases = await _releaseService.GetAsync(releaseIds);
            var publications = releases.Select(release => release.Publication).ToList();
            var methodologyIds = publications.Where(publication => publication.MethodologyId.HasValue)
                .Select(publication => publication.MethodologyId.Value).Distinct();

            foreach (var publication in publications)
            {
                // Only include the Publication if it's not already published
                if (!publication.Published.HasValue)
                {
                    await CachePublication(publication.Id, context, releaseIds);
                }

                await CacheLatestRelease(publication, context, releaseIds);
                foreach (var release in releases)
                {
                    await CacheFastTracks(release, context);
                    await CacheRelease(release, context);
                }
            }

            foreach (var methodologyId in methodologyIds)
            {
                await CacheMethodology(methodologyId, context);
            }
        }

        public async Task UpdatePublication(PublishContext context, Guid publicationId, string oldSlug)
        {
            var publication = await _publicationService.Get(publicationId);

            if (publication.Slug != oldSlug)
            {
                var pathPrefix = context.Staging ? PublicContentStagingPath() : null;
                await _fileStorageService.MovePublicDirectory(PublicContentContainerName,
                    PublicContentPublicationParentPath(oldSlug, pathPrefix),
                    PublicContentPublicationParentPath(publication.Slug, pathPrefix)
                );

                await _fileStorageService.MovePublicDirectory(PublicFilesContainerName, oldSlug, publication.Slug);
            }

            await CacheDownloadTree(context);
            await CachePublicationTree(context);
            await CachePublication(publication.Id, context);
        }

        private async Task DeleteAllContent()
        {
            await _fastTrackService.DeleteAllReleaseFastTracks();
            await _fileStorageService.DeleteAllContentAsyncExcludingStaging();
        }
        
        private async Task CacheDownloadTree(PublishContext context, params Guid[] includedReleaseIds)
        {
            // This assumes the files have been copied first
            var tree = _downloadService.GetTree(includedReleaseIds);
            await Upload(PublicContentDownloadTreePath, context, tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task CacheFastTracks(Release release, PublishContext context)
        {
            await _fastTrackService.CreateAllByRelease(release.Id, context);
        }
        
        private async Task CacheMethodologyTree(PublishContext context, params Guid[] includedReleaseIds)
        {
            var tree = _methodologyService.GetTree(includedReleaseIds);
            await Upload(PublicContentMethodologyTreePath, context, tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task CachePublicationTree(PublishContext context, params Guid[] includedReleaseIds)
        {
            var tree = _publicationService.GetTree(includedReleaseIds);
            await Upload(PublicContentPublicationsTreePath, context, tree, _jsonSerializerSettingsCamelCase);
        }

        private async Task CacheLatestRelease(Publication publication, PublishContext context, params Guid[] includedReleaseIds)
        {
            var viewModel = _releaseService.GetLatestReleaseViewModel(publication.Id, includedReleaseIds, context);
            await Upload(prefix => PublicContentLatestReleasePath(publication.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task CacheMethodology(Guid methodologyId, PublishContext context)
        {
            var viewModel = await _methodologyService.GetViewModelAsync(methodologyId, context);
            await Upload(prefix => PublicContentMethodologyPath(viewModel.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task CachePublication(Guid publicationId, PublishContext context, params Guid[] includedReleaseIds)
        {
            var viewModel = await _publicationService.GetViewModelAsync(publicationId, includedReleaseIds);
            await Upload(prefix => PublicContentPublicationPath(viewModel.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task CacheRelease(Release release, PublishContext context)
        {
            var viewModel = _releaseService.GetReleaseViewModel(release.Id, context);
            await Upload(prefix => PublicContentReleasePath(release.Publication.Slug, release.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task CacheTrees(PublishContext context, params Guid[] includedReleaseIds)
        {
            await CacheDownloadTree(context, includedReleaseIds);
            await CacheMethodologyTree(context, includedReleaseIds);
            await CachePublicationTree(context, includedReleaseIds);
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(NamingStrategy namingStrategy)
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = namingStrategy
                },
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }

        private async Task Upload(Func<string, string> pathFunction, PublishContext context, object value,
            JsonSerializerSettings settings)
        {
            var pathPrefix = context.Staging ? PublicContentStagingPath() : null;
            var blobName = pathFunction.Invoke(pathPrefix);
            await _fileStorageService.UploadAsJson(blobName, value, settings);
        }
    }
}