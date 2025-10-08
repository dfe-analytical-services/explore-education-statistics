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

                var publishingOrganisations = releaseVersion
                    .PublishingOrganisations.Select(PublishingOrganisationDto.FromOrganisation)
                    .OrderBy(o => o.Title)
                    .ToArray();

                var updateCount = await GetUpdateCount(releaseVersion.Id, cancellationToken);

                return ReleaseVersionSummaryDto.FromReleaseVersion(
                    releaseVersion,
                    isLatestRelease,
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

    private async Task<int> GetUpdateCount(Guid releaseVersionId, CancellationToken cancellationToken) =>
        await contentDbContext.Update.Where(u => u.ReleaseVersionId == releaseVersionId).CountAsync(cancellationToken)
        + 1; // +1 to include the 'First published' entry
}
