using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class ContentService : IContentService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IBlobCacheService _privateBlobCacheService;
    private readonly IBlobCacheService _publicBlobCacheService;
    private readonly IBlobStorageService _publicBlobStorageService;
    private readonly IReleaseService _releaseService;
    private readonly IMethodologyCacheService _methodologyCacheService;
    private readonly IPublicationsTreeService _publicationsTreeService;

    public ContentService(
        ContentDbContext contentDbContext,
        IBlobCacheService privateBlobCacheService,
        IBlobCacheService publicBlobCacheService,
        IBlobStorageService publicBlobStorageService,
        IReleaseService releaseService,
        IMethodologyCacheService methodologyCacheService,
        IPublicationsTreeService publicationsTreeService
    )
    {
        _contentDbContext = contentDbContext;
        _privateBlobCacheService = privateBlobCacheService;
        _publicBlobCacheService = publicBlobCacheService;
        _publicBlobStorageService = publicBlobStorageService;
        _releaseService = releaseService;
        _methodologyCacheService = methodologyCacheService;
        _publicationsTreeService = publicationsTreeService;
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
                releaseSlug: previousReleaseVersion.Release.Slug
            );
        }
    }

    private async Task DeleteLazilyCachedReleaseResults(
        Guid releaseVersionId,
        string publicationSlug,
        string releaseSlug
    )
    {
        await _privateBlobCacheService.DeleteCacheFolderAsync(
            new PrivateReleaseContentFolderCacheKey(releaseVersionId)
        );

        await _publicBlobCacheService.DeleteCacheFolderAsync(
            new ReleaseDataBlockResultsFolderCacheKey(publicationSlug, releaseSlug)
        );
        await _publicBlobCacheService.DeleteItemAsync(new ReleaseSubjectsCacheKey(publicationSlug, releaseSlug));
        await _publicBlobCacheService.DeleteCacheFolderAsync(
            new ReleaseSubjectMetaFolderCacheKey(publicationSlug, releaseSlug)
        );
    }

    public async Task DeletePreviousVersionsDownloadFiles(IReadOnlyList<Guid> releaseVersionIds)
    {
        var releaseVersions = await _contentDbContext
            .ReleaseVersions.Where(rv => releaseVersionIds.Contains(rv.Id))
            .Include(rv => rv.PreviousVersion)
            .ToListAsync();

        foreach (var releaseVersion in releaseVersions)
        {
            if (releaseVersion.PreviousVersion != null)
            {
                await _publicBlobStorageService.DeleteBlobs(
                    containerName: PublicReleaseFiles,
                    directoryPath: $"{releaseVersion.PreviousVersion.Id}/"
                );
            }
        }
    }

    public async Task UpdateCachedTaxonomyBlobs()
    {
        await _methodologyCacheService.UpdateSummariesTree();
        await _publicationsTreeService.UpdateCachedPublicationsTree();
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
