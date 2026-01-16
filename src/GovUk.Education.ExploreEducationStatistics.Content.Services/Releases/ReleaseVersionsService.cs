using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public class ReleaseVersionsService(ContentDbContext contentDbContext) : IReleaseVersionsService
{
    public async Task<Either<ActionResult, ReleaseVersionSummaryDto>> GetReleaseVersionSummary(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(publication =>
                GetLatestPublishedReleaseVersionByReleaseSlug(publication, releaseSlug, cancellationToken)
            )
            .OnSuccess(async releaseVersion =>
            {
                var isLatestRelease =
                    releaseVersion.Id == releaseVersion.Release.Publication.LatestPublishedReleaseVersionId;

                var lastUpdated =
                    releaseVersion.Version == 0
                        ? releaseVersion.Published!.Value
                        : await GetLastUpdatedFromReleaseUpdates(releaseVersion.Id, cancellationToken);

                var publishingOrganisations = releaseVersion
                    .PublishingOrganisations.Select(PublishingOrganisationDto.FromOrganisation)
                    .OrderBy(o => o.Title)
                    .ToArray();

                var updateCount = await GetUpdateCount(releaseVersion.Id, cancellationToken);

                return ReleaseVersionSummaryDto.FromReleaseVersion(
                    releaseVersion,
                    isLatestRelease,
                    lastUpdated,
                    publishingOrganisations,
                    updateCount
                );
            });

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .Publications.AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private Task<Either<ActionResult, ReleaseVersion>> GetLatestPublishedReleaseVersionByReleaseSlug(
        Publication publication,
        string releaseSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Include(rv => rv.Release.Publication)
            .Include(rv => rv.PublishingOrganisations)
            .LatestReleaseVersions(publication.Id, releaseSlug, publishedOnly: true)
            .SingleOrNotFoundAsync(cancellationToken);

    /// <summary>
    /// Until a change in EES-6414 makes <see cref="ReleaseVersion.Published"/> the effective public facing 'last updated'
    /// date of the release, use the date from the latest release update, in the same way the frontend did for the old
    /// release page design.
    /// </summary>
    /// <param name="releaseVersionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The date of the latest update for the release version.</returns>
    private async Task<DateTimeOffset> GetLastUpdatedFromReleaseUpdates(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    )
    {
        var lastUpdated = await contentDbContext
            .Update.Where(u => u.ReleaseVersionId == releaseVersionId)
            .OrderByDescending(u => u.On)
            .Select(u => u.On)
            .FirstOrDefaultAsync(cancellationToken);

        if (lastUpdated == default)
        {
            throw new InvalidOperationException(
                $"Expected release version '{releaseVersionId}' to have Updates to determine last updated date from."
            );
        }

        // EES-6490 has been created to convert Update.On from DateTime to DateTimeOffset but until then, convert it here.
        // Values of Update.On are created by ReleaseNoteService in local time using DateTime.Now,
        // even though it's standard elsewhere across the service to store and return dates as UTC.
        // Interpret the value as local time, create a DateTimeOffset with the correct UTC offset for the local time zone,
        // then convert it to UTC so it is consistent with other dates.
        var localOffset = TimeZoneInfo.Local.GetUtcOffset(lastUpdated);
        return new DateTimeOffset(lastUpdated, localOffset).ToUniversalTime();
    }

    private async Task<int> GetUpdateCount(Guid releaseVersionId, CancellationToken cancellationToken) =>
        await contentDbContext.Update.Where(u => u.ReleaseVersionId == releaseVersionId).CountAsync(cancellationToken)
        + 1; // +1 to include the 'First published' entry
}
