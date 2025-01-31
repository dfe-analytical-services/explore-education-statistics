using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ContentService : IContentService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IBlobCacheService _privateBlobCacheService;
        private readonly IBlobCacheService _publicBlobCacheService;
        private readonly IBlobStorageService _publicBlobStorageService;
        private readonly IReleaseService _releaseService;
        private readonly IMethodologyCacheService _methodologyCacheService;
        private readonly IReleaseCacheService _releaseCacheService;
        private readonly IPublicationCacheService _publicationCacheService;

        public ContentService(
            ContentDbContext contentDbContext,
            IBlobCacheService privateBlobCacheService,
            IBlobCacheService publicBlobCacheService,
            IBlobStorageService publicBlobStorageService,
            IReleaseService releaseService,
            IMethodologyCacheService methodologyCacheService,
            IReleaseCacheService releaseCacheService,
            IPublicationCacheService publicationCacheService)
        {
            _contentDbContext = contentDbContext;
            _privateBlobCacheService = privateBlobCacheService;
            _publicBlobCacheService = publicBlobCacheService;
            _publicBlobStorageService = publicBlobStorageService;
            _releaseService = releaseService;
            _methodologyCacheService = methodologyCacheService;
            _releaseCacheService = releaseCacheService;
            _publicationCacheService = publicationCacheService;
        }

        public async Task DeletePreviousVersionsContent(IReadOnlyList<Guid> releaseVersionIds)
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
                await DeleteLazilyCachedReleaseResults(
                    releaseVersionId: previousReleaseVersion.Id,
                    publicationSlug: releaseVersion.Release.Publication.Slug,
                    releaseSlug: previousReleaseVersion.Release.Slug);
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

        public async Task DeletePreviousVersionsDownloadFiles(IReadOnlyList<Guid> releaseVersionIds)
        {
            var releaseVersions = await _contentDbContext.ReleaseVersions
                .Where(rv => releaseVersionIds.Contains(rv.Id))
                .Include(rv => rv.PreviousVersion)
                .ToListAsync();

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

        public async Task UpdateContent(Guid releaseVersionId)
        {
            var releaseVersion = await _contentDbContext.ReleaseVersions
                .Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
                .SingleAsync(rv => rv.Id == releaseVersionId);

            await _releaseCacheService.UpdateRelease(
                releaseVersion.Id,
                publicationSlug: releaseVersion.Release.Publication.Slug,
                releaseSlug: releaseVersion.Release.Slug);

            var publication = releaseVersion.Release.Publication;

            // Cache the latest release version for the publication as a separate cache entry
            var latestReleaseVersion = await _releaseService.GetLatestPublishedReleaseVersion(
                publicationId: publication.Id,
                includeUnpublishedVersionIds: [releaseVersion.Id]);

            await _releaseCacheService.UpdateRelease(
                releaseVersionId: latestReleaseVersion.Id,
                publicationSlug: publication.Slug);
        }

        public async Task UpdateContentStaged(
            DateTime expectedPublishDate,
            params Guid[] releaseVersionIds)
        {
            var releaseVersions = await _contentDbContext.ReleaseVersions
                .Where(rv => releaseVersionIds.Contains(rv.Id))
                .Include(rv => rv.Release)
                .ThenInclude(r => r.Publication)
                .ToListAsync();

            foreach (var releaseVersion in releaseVersions)
            {
                await _releaseCacheService.UpdateReleaseStaged(
                    releaseVersion.Id,
                    expectedPublishDate,
                    publicationSlug: releaseVersion.Release.Publication.Slug,
                    releaseSlug: releaseVersion.Release.Slug);
            }

            var publications = releaseVersions
                .Select(rv => rv.Release.Publication)
                .DistinctBy(p => p.Id)
                .ToList();

            foreach (var publication in publications)
            {
                // Cache the latest release version for the publication as a separate cache entry
                var latestReleaseVersion = await _releaseService.GetLatestPublishedReleaseVersion(
                        publicationId: publication.Id,
                        includeUnpublishedVersionIds: releaseVersionIds);

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
