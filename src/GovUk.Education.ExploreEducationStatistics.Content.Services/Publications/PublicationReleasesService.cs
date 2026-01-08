using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
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
                var releaseVersionsByReleaseId = await GetPublishedReleaseVersions(publication, cancellationToken);

                var lastUpdatedDatesByReleaseId = GetReleaseLastUpdatedDates(releaseVersionsByReleaseId);

                // Release entries provide the order of releases in a publication and also include any legacy releases
                var releaseEntries = GetPublishedOrLegacyReleaseEntries(publication, releaseVersionsByReleaseId);

                var latestReleaseEntry = releaseEntries.OfType<PublicationReleaseEntry>().FirstOrDefault();

                // Map to DTO's in the same order as the publication's release entries, before paginating in-memory
                var entryDtos = MapToPublicationReleaseEntryDtos(
                    releaseEntries,
                    releaseVersionsByReleaseId,
                    lastUpdatedDatesByReleaseId,
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
            // Include all release updates here until EES-6414, as they are needed to determine 'last updated' for non-first versions
            .Include(rv => rv.Updates)
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

    private Dictionary<Guid, DateTimeOffset> GetReleaseLastUpdatedDates(
        Dictionary<Guid, ReleaseVersion> releaseVersionsByReleaseId
    ) =>
        releaseVersionsByReleaseId.ToDictionary(
            kvp => kvp.Key,
            r =>
            {
                var releaseVersion = r.Value;
                return releaseVersion.Version == 0
                    ? releaseVersion.Published!.Value
                    : GetLastUpdatedFromReleaseUpdates(releaseVersion);
            }
        );

    /// <summary>
    /// Until a change in EES-6414 makes <see cref="ReleaseVersion.Published"/> the effective public facing 'last updated'
    /// date of the release, use the date from the latest release update, in the same way the frontend did for the old
    /// release page design.
    /// </summary>
    /// <param name="releaseVersion"></param>
    /// <returns>The date of the latest update for the release version.</returns>
    private DateTimeOffset GetLastUpdatedFromReleaseUpdates(ReleaseVersion releaseVersion)
    {
        var lastUpdated = releaseVersion.Updates.OrderByDescending(u => u.On).Select(u => u.On).FirstOrDefault();

        if (lastUpdated == default)
        {
            throw new InvalidOperationException(
                $"Expected release version '{releaseVersion.Id}' to have Updates to determine last updated date from."
            );
        }

        // EES-6490 has been created to convert Update.On from DateTime to DateTimeOffset but until then, convert it here.
        // Values of Update.On are created by ReleaseNoteService in local time using DateTime.Now,
        // even though it's standard elsewhere across the service to store and return dates as UTC.
        // Interpret the value as UK local time, create a DateTimeOffset with the correct UTC offset for the UK time zone,
        // then convert it to UTC so it is consistent with other dates.
        var ukZoneOffset = TimeZoneUtils.GetUkTimeZone().GetUtcOffset(lastUpdated);
        return new DateTimeOffset(lastUpdated, ukZoneOffset).ToUniversalTime();
    }

    private static List<IPublicationReleaseEntryDto> MapToPublicationReleaseEntryDtos(
        List<IPublicationReleaseEntry> releaseEntries,
        Dictionary<Guid, ReleaseVersion> releaseVersionsByReleaseId,
        Dictionary<Guid, DateTimeOffset> lastUpdatedDatesByReleaseId,
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
                        lastUpdated: lastUpdatedDatesByReleaseId[releaseEntry.ReleaseId],
                        published: releaseVersionsByReleaseId[releaseEntry.ReleaseId].PublishedDisplayDate!.Value
                    ),

                    _ => throw new ArgumentOutOfRangeException(nameof(e)),
                }
            )
            .ToList();
}
