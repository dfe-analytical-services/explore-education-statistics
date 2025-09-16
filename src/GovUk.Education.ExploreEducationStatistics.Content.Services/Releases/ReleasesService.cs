using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public class ReleasesService(ContentDbContext contentDbContext) : IReleasesService
{
    public async Task<Either<ActionResult, PaginatedListViewModel<IReleaseEntryDto>>>
        GetPaginatedReleaseEntriesForPublication(
            string publicationSlug,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(async publication =>
            {
                var publishedReleaseVersions =
                    await GetPublishedReleaseVersions(publication, cancellationToken);

                var releaseEntries =
                    GetPublishedOrLegacyReleaseEntries(publication, publishedReleaseVersions);

                var latestReleaseEntry = releaseEntries
                    .OfType<ReleaseEntry>()
                    .FirstOrDefault();

                // Map to DTO's in the same order as the publication's release entries, before paginating in-memory
                var summaries = MapToReleaseEntryDtos(
                    releaseEntries,
                    publishedReleaseVersions,
                    latestReleaseEntry);

                return PaginatedListViewModel<IReleaseEntryDto>.Paginate(summaries, page, pageSize);
            });

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken) =>
        contentDbContext.Publications
            .AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private async Task<Dictionary<Guid, ReleaseVersion>> GetPublishedReleaseVersions(
        Publication publication,
        CancellationToken cancellationToken) =>
        await contentDbContext.ReleaseVersions
            .AsNoTracking()
            .Include(rv => rv.Release)
            .LatestReleaseVersions(publicationId: publication.Id, publishedOnly: true)
            // There should only be one version per release since only the latest published versions are selected
            .ToDictionaryAsync(rv => rv.ReleaseId, rv => rv, cancellationToken);

    private static List<IPublicationReleaseEntry> GetPublishedOrLegacyReleaseEntries(
        Publication publication,
        Dictionary<Guid, ReleaseVersion> publishedReleaseVersionsByReleaseId) =>
        publication.ReleaseEntries
            .Where(e => e is LegacyReleaseEntry ||
                        (e is ReleaseEntry release &&
                         publishedReleaseVersionsByReleaseId.ContainsKey(release.ReleaseId)))
            .ToList();

    private static List<IReleaseEntryDto> MapToReleaseEntryDtos(
        List<IPublicationReleaseEntry> releaseEntries,
        Dictionary<Guid, ReleaseVersion> releaseVersionsByReleaseId,
        ReleaseEntry? latestReleaseEntry) =>
        releaseEntries
            .Select<IPublicationReleaseEntry, IReleaseEntryDto>(e => e switch
            {
                LegacyReleaseEntry legacyReleaseEntry =>
                    LegacyReleaseEntryDto.FromLegacyReleaseEntry(legacyReleaseEntry),

                ReleaseEntry releaseEntry =>
                    ReleaseEntryDto.FromRelease(
                        releaseVersionsByReleaseId[releaseEntry.ReleaseId].Release,
                        isLatestRelease: releaseEntry == latestReleaseEntry,
                        lastUpdated: releaseVersionsByReleaseId[releaseEntry.ReleaseId].Published!.Value,
                        // TODO EES-6414 'Published' should be the published display date
                        published: releaseVersionsByReleaseId[releaseEntry.ReleaseId].Published!.Value),

                _ => throw new ArgumentOutOfRangeException(nameof(e))
            })
            .ToList();
}
