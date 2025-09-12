using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public class ReleaseUpdatesService(ContentDbContext contentDbContext) : IReleaseUpdatesService
{
    public async Task<Either<ActionResult, PaginatedListViewModel<ReleaseUpdateDto>>> GetPaginatedUpdatesForRelease(
        string publicationSlug,
        string releaseSlug,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(publication =>
                GetLatestPublishedReleaseVersionByReleaseSlug(
                    publication,
                    releaseSlug,
                    cancellationToken))
            .OnSuccess(async releaseVersion =>
                await GetPaginatedUpdatesForReleaseVersion(
                    releaseVersion,
                    page,
                    pageSize,
                    cancellationToken));

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken) =>
        contentDbContext.Publications
            .AsNoTracking()
            .Where(p => p.Slug == publicationSlug && p.LatestPublishedReleaseVersionId.HasValue)
            .SingleOrNotFoundAsync(cancellationToken);

    private Task<Either<ActionResult, ReleaseVersion>> GetLatestPublishedReleaseVersionByReleaseSlug(
        Publication publication,
        string releaseSlug,
        CancellationToken cancellationToken) =>
        contentDbContext.ReleaseVersions
            .AsNoTracking()
            .LatestReleaseVersions(publication.Id, releaseSlug, publishedOnly: true)
            .SingleOrNotFoundAsync(cancellationToken);

    private async Task<PaginatedListViewModel<ReleaseUpdateDto>> GetPaginatedUpdatesForReleaseVersion(
        ReleaseVersion releaseVersion,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var updates = await contentDbContext.Update
            .AsNoTracking()
            .Where(u => u.ReleaseVersionId == releaseVersion.Id)
            .Select(u => ReleaseUpdateDto.FromUpdate(u))
            .ToListAsync(cancellationToken);

        var firstPublishedUpdate = new ReleaseUpdateDto
        {
            Date = await GetReleaseFirstPublishedDate(releaseVersion),
            Summary = "First published"
        };

        var allUpdates = updates
            .Append(firstPublishedUpdate)
            .OrderByDescending(u => u.Date)
            .ToList();

        // Pagination is applied in-memory since the 'First published' entry is combined with database results
        return PaginatedListViewModel<ReleaseUpdateDto>.Paginate(allUpdates, page, pageSize);
    }

    private async Task<DateTime> GetReleaseFirstPublishedDate(ReleaseVersion releaseVersion)
    {
        var published = releaseVersion.Version == 0
            ? releaseVersion.Published
            : await contentDbContext.ReleaseVersions
                .Where(rv => rv.ReleaseId == releaseVersion.ReleaseId && rv.Version == 0)
                .Select(rv => rv.Published)
                .SingleAsync();
        return published!.Value;
    }
}
