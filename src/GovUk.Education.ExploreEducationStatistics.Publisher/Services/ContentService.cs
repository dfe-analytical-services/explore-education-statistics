#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentService : IContentService
    {
        private readonly IBlobCacheService _privateBlobCacheService;
        private readonly IBlobCacheService _publicBlobCacheService;
        private readonly IBlobStorageService _publicBlobStorageService;
        private readonly IReleaseService _releaseService;
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IReleaseCacheService _releaseCacheService;
        private readonly IPublicationCacheService _publicationCacheService;

        public ContentService(
            IBlobCacheService privateBlobCacheService,
            IBlobCacheService publicBlobCacheService,
            IBlobStorageService publicBlobStorageService,
            IReleaseService releaseService,
            IMethodologyCacheService methodologyCacheService,
            IReleaseCacheService releaseCacheService,
            IPublicationCacheService publicationCacheService)
        {
            _privateBlobCacheService = privateBlobCacheService;
            _publicBlobCacheService = publicBlobCacheService;
            _publicBlobStorageService = publicBlobStorageService;
            _releaseService = releaseService;
            _methodologyCacheService = methodologyCacheService;
            _releaseCacheService = releaseCacheService;
            _publicationCacheService = publicationCacheService;
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

                // Delete any lazily-cached results that are owned by the previous Release
                await DeleteLazilyCachedReleaseResults(previousRelease.Id, release.Publication.Slug, previousRelease.Slug);

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

        private async Task DeleteLazilyCachedReleaseResults(Guid releaseId, string publicationSlug, string releaseSlug)
        {
            await _privateBlobCacheService.DeleteCacheFolder(new PrivateReleaseContentFolderCacheKey(releaseId));

            await _publicBlobCacheService.DeleteCacheFolder(new ReleaseDataBlockResultsFolderCacheKey(publicationSlug, releaseSlug));
            await _publicBlobCacheService.DeleteItem(new ReleaseSubjectsCacheKey(publicationSlug, releaseSlug));
            await _publicBlobCacheService.DeleteCacheFolder(new ReleaseSubjectMetaFolderCacheKey(publicationSlug, releaseSlug));
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

        public async Task UpdateContent(params Guid[] releaseIds)
        {
            var releases = (await _releaseService
                    .List(releaseIds))
                .ToList();

            foreach (var release in releases)
            {
                await _releaseCacheService.UpdateRelease(
                    release.Id,
                    publicationSlug: release.Publication.Slug,
                    releaseSlug: release.Slug);
            }

            var publications = releases
                .Select(release => release.Publication)
                .DistinctByProperty(publication => publication.Id)
                .ToList();

            foreach (var publication in publications)
            {
                // Cache the latest release for the publication as a separate cache entry
                var latestRelease = await _releaseService.GetLatestRelease(publication.Id, releaseIds);
                await _releaseCacheService.UpdateRelease(
                    latestRelease.Id,
                    publicationSlug: publication.Slug);
            }
        }

        public async Task UpdateContentStaged(DateTime expectedPublishDate, params Guid[] releaseIds)
        {
            var releases = (await _releaseService
                    .List(releaseIds))
                .ToList();

            foreach (var release in releases)
            {
                await _releaseCacheService.UpdateReleaseStaged(
                    release.Id,
                    expectedPublishDate,
                    publicationSlug: release.Publication.Slug,
                    releaseSlug: release.Slug);
            }

            var publications = releases
                .Select(release => release.Publication)
                .DistinctByProperty(publication => publication.Id)
                .ToList();

            foreach (var publication in publications)
            {
                // Cache the latest release for the publication as a separate cache entry
                var latestRelease = await _releaseService.GetLatestRelease(publication.Id, releaseIds);
                await _releaseCacheService.UpdateReleaseStaged(
                    latestRelease.Id,
                    expectedPublishDate,
                    publicationSlug: publication.Slug);
            }
        }

        public async Task UpdateCachedTaxonomyBlobs()
        {
            await _methodologyCacheService.UpdateSummariesTree();
            await _publicationCacheService.UpdatePublicationTree();
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
