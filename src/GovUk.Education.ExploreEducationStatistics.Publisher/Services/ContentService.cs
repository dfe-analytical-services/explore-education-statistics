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

        public async Task DeletePreviousVersionsContent(params Guid[] releaseVersionIds)
        {
            var releaseVersions = await _releaseService.GetAmendedReleases(releaseVersionIds);

            foreach (var releaseVersion in releaseVersions)
            {
                var previousReleaseVersion = releaseVersion.PreviousVersion;

                if (previousReleaseVersion == null)
                {
                    break;
                }

                // Delete any lazily-cached results that are owned by the previous Release
                await DeleteLazilyCachedReleaseResults(previousReleaseVersion.Id, releaseVersion.Publication.Slug, previousReleaseVersion.Slug);

                // Delete content which hasn't been overwritten because the Slug has changed
                if (releaseVersion.Slug != previousReleaseVersion.Slug)
                {
                    await _publicBlobStorageService.DeleteBlob(
                        PublicContent,
                        PublicContentReleasePath(releaseVersion.Publication.Slug, previousReleaseVersion.Slug)
                    );
                }
            }
        }

        private async Task DeleteLazilyCachedReleaseResults(Guid releaseVersionId,
            string publicationSlug,
            string releaseSlug)
        {
            await _privateBlobCacheService.DeleteCacheFolderAsync(new PrivateReleaseContentFolderCacheKey(releaseVersionId));

            await _publicBlobCacheService.DeleteCacheFolderAsync(new ReleaseDataBlockResultsFolderCacheKey(publicationSlug, releaseSlug));
            await _publicBlobCacheService.DeleteItemAsync(new ReleaseSubjectsCacheKey(publicationSlug, releaseSlug));
            await _publicBlobCacheService.DeleteCacheFolderAsync(new ReleaseSubjectMetaFolderCacheKey(publicationSlug, releaseSlug));
        }

        public async Task DeletePreviousVersionsDownloadFiles(params Guid[] releaseVersionIds)
        {
            var releaseVersions = await _releaseService.List(releaseVersionIds);

            foreach (var releaseVersion in releaseVersions)
            {
                if (releaseVersion.PreviousVersion != null)
                {
                    await _publicBlobStorageService.DeleteBlobs(
                        containerName: PublicReleaseFiles,
                        directoryPath: $"{releaseVersion.PreviousVersion.Id}/");
                }
            }
        }

        public async Task UpdateContent(params Guid[] releaseVersionIds)
        {
            var releaseVersions = (await _releaseService
                    .List(releaseVersionIds))
                .ToList();

            foreach (var releaseVersion in releaseVersions)
            {
                await _releaseCacheService.UpdateRelease(
                    releaseVersion.Id,
                    publicationSlug: releaseVersion.Publication.Slug,
                    releaseSlug: releaseVersion.Slug);
            }

            var publications = releaseVersions
                .Select(rv => rv.Publication)
                .DistinctByProperty(publication => publication.Id)
                .ToList();

            foreach (var publication in publications)
            {
                // Cache the latest release version for the publication as a separate cache entry
                var latestReleaseVersion = await _releaseService.GetLatestReleaseVersion(publication.Id, releaseVersionIds);
                await _releaseCacheService.UpdateRelease(
                    latestReleaseVersion.Id,
                    publicationSlug: publication.Slug);
            }
        }

        public async Task UpdateContentStaged(DateTime expectedPublishDate,
            params Guid[] releaseVersionIds)
        {
            var releaseVersions = (await _releaseService
                    .List(releaseVersionIds))
                .ToList();

            foreach (var releaseVersion in releaseVersions)
            {
                await _releaseCacheService.UpdateReleaseStaged(
                    releaseVersion.Id,
                    expectedPublishDate,
                    publicationSlug: releaseVersion.Publication.Slug,
                    releaseSlug: releaseVersion.Slug);
            }

            var publications = releaseVersions
                .Select(rv => rv.Publication)
                .DistinctByProperty(publication => publication.Id)
                .ToList();

            foreach (var publication in publications)
            {
                // Cache the latest release version for the publication as a separate cache entry
                var latestReleaseVersion = await _releaseService.GetLatestReleaseVersion(publication.Id, releaseVersionIds);
                await _releaseCacheService.UpdateReleaseStaged(
                    latestReleaseVersion.Id,
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
