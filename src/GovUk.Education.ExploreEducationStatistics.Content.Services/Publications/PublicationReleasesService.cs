using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public class PublicationReleasesService(ContentDbContext contentDbContext) : IPublicationReleasesService
{
    public async Task<Either<ActionResult, PaginatedListViewModel<IPublicationReleaseEntryDto>>> GetPublicationReleases(
        string publicationSlug,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(async publication =>
            {
                var publishedReleaseVersions = await GetPublishedReleaseVersions(publication, cancellationToken);

                var releaseEntries = GetPublishedOrLegacyReleaseEntries(publication, publishedReleaseVersions);

                var latestReleaseEntry = releaseEntries.OfType<PublicationReleaseEntry>().FirstOrDefault();

                // Map to DTO's in the same order as the publication's release entries, before paginating in-memory
                var entryDtos = MapToPublicationReleaseEntryDtos(
                    releaseEntries,
                    publishedReleaseVersions,
                    latestReleaseEntry
                );

                return PaginatedListViewModel<IPublicationReleaseEntryDto>.Paginate(entryDtos, page, pageSize);
            });

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .Publications.AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private async Task<Dictionary<Guid, ReleaseVersion>> GetPublishedReleaseVersions(
        Publication publication,
        CancellationToken cancellationToken
    ) =>
        await contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Include(rv => rv.Release)
            .LatestReleaseVersions(publicationId: publication.Id, publishedOnly: true)
            // There should only be one version per release since only the latest published versions are selected
            .ToDictionaryAsync(rv => rv.ReleaseId, rv => rv, cancellationToken);

    private static List<IPublicationReleaseEntry> GetPublishedOrLegacyReleaseEntries(
        Publication publication,
        Dictionary<Guid, ReleaseVersion> publishedReleaseVersionsByReleaseId
    ) =>
        publication
            .ReleaseEntries.Where(e =>
                e is LegacyPublicationReleaseEntry
                || (
                    e is PublicationReleaseEntry release
                    && publishedReleaseVersionsByReleaseId.ContainsKey(release.ReleaseId)
                )
            )
            .ToList();

    private static List<IPublicationReleaseEntryDto> MapToPublicationReleaseEntryDtos(
        List<IPublicationReleaseEntry> releaseEntries,
        Dictionary<Guid, ReleaseVersion> releaseVersionsByReleaseId,
        PublicationReleaseEntry? latestReleaseEntry
    ) =>
        releaseEntries
            .Select<IPublicationReleaseEntry, IPublicationReleaseEntryDto>(e =>
                e switch
                {
                    LegacyPublicationReleaseEntry releaseEntry =>
                        LegacyPublicationReleaseEntryDto.FromLegacyPublicationReleaseEntry(releaseEntry),

                    PublicationReleaseEntry releaseEntry => PublicationReleaseEntryDto.FromRelease(
                        releaseVersionsByReleaseId[releaseEntry.ReleaseId].Release,
                        isLatestRelease: releaseEntry == latestReleaseEntry,
                        lastUpdated: releaseVersionsByReleaseId[releaseEntry.ReleaseId].Published!.Value,
                        // TODO EES-6414 'Published' should be the published display date
                        published: releaseVersionsByReleaseId[releaseEntry.ReleaseId].Published!.Value
                    ),

                    _ => throw new ArgumentOutOfRangeException(nameof(e)),
                }
            )
            .ToList();
}
