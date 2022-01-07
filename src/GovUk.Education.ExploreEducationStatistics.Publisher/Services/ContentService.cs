using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.IBlobStorageService;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentService : IContentService
    {
        private readonly IBlobCacheService _blobCacheService;
        private readonly IBlobStorageService _publicBlobStorageService;
        private readonly IFastTrackService _fastTrackService;
        private readonly IReleaseService _releaseService;
        private readonly IPublicationService _publicationService;

        private readonly JsonSerializerSettings _jsonSerializerSettingsCamelCase =
            GetJsonSerializerSettings(new CamelCaseNamingStrategy());

        public ContentService(IBlobStorageService publicBlobStorageService,
            IBlobCacheService blobCacheService,
            IFastTrackService fastTrackService,
            IReleaseService releaseService,
            IPublicationService publicationService)
        {
            _publicBlobStorageService = publicBlobStorageService;
            _blobCacheService = blobCacheService;
            _fastTrackService = fastTrackService;
            _releaseService = releaseService;
            _publicationService = publicationService;
        }

        public async Task DeletePreviousVersionsContent(params Guid[] releaseIds)
        {
            var releases = await _releaseService.GetAmendedReleases(releaseIds);

            foreach (var release in releases)
            {
                var previousRelease = release.PreviousVersion;

                if (previousRelease == null)
                {
                    break;
                }

                await _fastTrackService.DeleteAllFastTracksByRelease(previousRelease.Id);

                // Delete any lazily-cached results that are owned by the previous Release
                await DeleteLazilyCachedReleaseResults(release.Publication.Slug, previousRelease.Slug);

                // Delete content which hasn't been overwritten because the Slug has changed
                if (release.Slug != previousRelease.Slug)
                {
                    await _publicBlobStorageService.DeleteBlob(
                        PublicContent,
                        PublicContentReleasePath(release.Publication.Slug, previousRelease.Slug)
                    );
                }
            }
        }

        private async Task DeleteLazilyCachedReleaseResults(string publicationSlug, string releaseSlug)
        {
            await _blobCacheService.DeleteCacheFolder(new ReleaseDataBlockResultsFolderCacheKey(publicationSlug, releaseSlug));
            await _blobCacheService.DeleteItem(new ReleaseSubjectsCacheKey(publicationSlug, releaseSlug));
            await _blobCacheService.DeleteCacheFolder(new ReleaseFastTrackResultsFolderCacheKey(publicationSlug, releaseSlug));
            await _blobCacheService.DeleteCacheFolder(new ReleaseSubjectMetaFolderCacheKey(publicationSlug, releaseSlug));
        }

        public async Task DeletePreviousVersionsDownloadFiles(params Guid[] releaseIds)
        {
            var releases = await _releaseService.List(releaseIds);

            foreach (var release in releases)
            {
                if (release.PreviousVersion != null)
                {
                    await _publicBlobStorageService.DeleteBlobs(
                        containerName: PublicReleaseFiles,
                        directoryPath: $"{release.PreviousVersion.Id}/");
                }
            }
        }

        /**
         * Intended to be used as a Development / BAU Function to perform a full content refresh
         */
        public async Task UpdateAllContentAsync()
        {
            var context = new PublishContext(DateTime.UtcNow, false);

            await DeleteAllContent();

            var publications = _publicationService.GetPublicationsWithPublishedReleases();

            foreach (var publication in publications)
            {
                var releases = publication.Releases.Where(release => release.IsLatestPublishedVersionOfRelease());
                await CachePublication(publication.Id, context);
                await CacheLatestRelease(publication, context);
                foreach (var release in releases)
                {
                    await CacheFastTracks(release, context);
                    await CacheRelease(release, context);
                }
            }
        }

        public async Task UpdateContent(PublishContext context, params Guid[] releaseIds)
        {
            var releases = (await _releaseService
                .List(releaseIds))
                .ToList();

            var publications = releases
                .Select(release => release.Publication)
                .DistinctByProperty(publication => publication.Id)
                .ToList();

            foreach (var publication in publications)
            {
                await CachePublication(publication.Id, context, releaseIds);
                await CacheLatestRelease(publication, context, releaseIds);
                foreach (var release in releases)
                {
                    await CacheFastTracks(release, context);
                    await CacheRelease(release, context);
                }
            }
        }

        public async Task UpdatePublication(PublishContext context, Guid publicationId)
        {
            var publication = await _publicationService.Get(publicationId);

            await CachePublication(publication.Id, context);
            await _publicationService.SetPublishedDate(publication.Id, context.Published);

            // Invalidate the various cached trees in case any
            // publications/methodologies are affected by the changes
            await DeleteCachedTaxonomyBlobs();
        }

        public async Task DeleteCachedTaxonomyBlobs()
        {
            await _blobCacheService.DeleteItem(new AllMethodologiesCacheKey());
            await _blobCacheService.DeleteItem(new PublicationTreeCacheKey());
            await _blobCacheService.DeleteItem(new PublicationTreeCacheKey(PublicationTreeFilter.AnyData));
            await _blobCacheService.DeleteItem(new PublicationTreeCacheKey(PublicationTreeFilter.LatestData));
        }

        private async Task DeleteAllContent()
        {
            await _fastTrackService.DeleteAllReleaseFastTracks();
            await DeleteAllContentAsyncExcludingStaging();
        }

        private async Task CacheFastTracks(Release release, PublishContext context)
        {
            await _fastTrackService.CreateAllByRelease(release.Id, context);
        }

        private async Task CacheLatestRelease(Publication publication, PublishContext context, params Guid[] includedReleaseIds)
        {
            var viewModel = await _releaseService.GetLatestReleaseViewModel(publication.Id, includedReleaseIds, context);
            await Upload(prefix => PublicContentLatestReleasePath(publication.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task CachePublication(Guid publicationId, PublishContext context, params Guid[] includedReleaseIds)
        {
            var viewModel = await _publicationService.GetViewModel(publicationId, includedReleaseIds);
            await Upload(prefix => PublicContentPublicationPath(viewModel.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task CacheRelease(Release release, PublishContext context)
        {
            var viewModel = await _releaseService.GetReleaseViewModel(release.Id, context);
            await Upload(prefix => PublicContentReleasePath(release.Publication.Slug, release.Slug, prefix), context, viewModel, _jsonSerializerSettingsCamelCase);
        }

        private async Task DeleteAllContentAsyncExcludingStaging()
        {
            await _publicBlobStorageService.DeleteBlobs(
                containerName: PublicContent,
                options: new DeleteBlobsOptions
                {
                    ExcludeRegex = new Regex($"^{PublicContentStagingPath()}/.+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
                }
            );
        }

        private static JsonSerializerSettings GetJsonSerializerSettings(NamingStrategy namingStrategy)
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = namingStrategy,
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
            await _publicBlobStorageService.UploadAsJson(PublicContent, blobName, value, settings);
        }

        private record ReleaseDataBlockResultsFolderCacheKey : IBlobCacheKey
        {
            private string PublicationSlug { get; }

            private string ReleaseSlug { get; }

            public ReleaseDataBlockResultsFolderCacheKey(string publicationSlug, string releaseSlug)
            {
                PublicationSlug = publicationSlug;
                ReleaseSlug = releaseSlug;
            }

            public string Key => PublicContentDataBlockParentPath(PublicationSlug, ReleaseSlug);

            public IBlobContainer Container => PublicContent;
        }

        private record ReleaseSubjectsCacheKey : IBlobCacheKey
        {
            private string PublicationSlug { get; }

            private string ReleaseSlug { get; }

            public ReleaseSubjectsCacheKey(string publicationSlug, string releaseSlug)
            {
                PublicationSlug = publicationSlug;
                ReleaseSlug = releaseSlug;
            }

            public string Key => PublicContentReleaseSubjectsPath(PublicationSlug, ReleaseSlug);

            public IBlobContainer Container => PublicContent;
        }

        private record ReleaseFastTrackResultsFolderCacheKey : IBlobCacheKey
        {
            private string PublicationSlug { get; }

            private string ReleaseSlug { get; }

            public ReleaseFastTrackResultsFolderCacheKey(string publicationSlug, string releaseSlug)
            {
                PublicationSlug = publicationSlug;
                ReleaseSlug = releaseSlug;
            }

            public string Key => PublicContentFastTrackResultsParentPath(PublicationSlug, ReleaseSlug);

            public IBlobContainer Container => PublicContent;
        }

        private record ReleaseSubjectMetaFolderCacheKey : IBlobCacheKey
        {
            private string PublicationSlug { get; }

            private string ReleaseSlug { get; }

            public ReleaseSubjectMetaFolderCacheKey(string publicationSlug, string releaseSlug)
            {
                PublicationSlug = publicationSlug;
                ReleaseSlug = releaseSlug;
            }

            public string Key => PublicContentSubjectMetaParentPath(PublicationSlug, ReleaseSlug);

            public IBlobContainer Container => PublicContent;
        }
    }
}
