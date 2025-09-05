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
        GetReleaseUpdatesRequest request,
        CancellationToken cancellationToken = default)
    {
        return await GetPublicationBySlug(request.PublicationSlug, cancellationToken)
            .OnSuccess(publication =>
                GetLatestPublishedReleaseVersionByReleaseSlug(
                    publication,
                    request.ReleaseSlug,
                    cancellationToken))
            .OnSuccess(async releaseVersion =>
                await GetPaginatedUpdatesForReleaseVersion(
                    releaseVersion.Id,
                    request.Page,
                    request.PageSize,
                    cancellationToken));
    }

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken = default)
    {
        return contentDbContext.Publications
            .Where(p => p.Slug == publicationSlug && p.LatestPublishedReleaseVersionId.HasValue)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    private Task<Either<ActionResult, ReleaseVersion>> GetLatestPublishedReleaseVersionByReleaseSlug(
        Publication publication,
        string releaseSlug,
        CancellationToken cancellationToken = default)
    {
        return contentDbContext.ReleaseVersions
            .LatestReleaseVersions(publication.Id, releaseSlug, publishedOnly: true)
            .SingleOrNotFoundAsync(cancellationToken);
    }

    private async Task<PaginatedListViewModel<ReleaseUpdateDto>> GetPaginatedUpdatesForReleaseVersion(
        Guid releaseVersionId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var updatesQuery = contentDbContext.Update
            .Where(u => u.ReleaseVersionId == releaseVersionId)
            .OrderByDescending(u => u.On);

        var totalResults = await updatesQuery.CountAsync(cancellationToken);

        var updates = await updatesQuery
            .Paginate(page: page, pageSize: pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedListViewModel<ReleaseUpdateDto>(
            page: page,
            pageSize: pageSize,
            results: updates.Select(ReleaseUpdateDto.FromUpdate).ToList(),
            totalResults: totalResults);
    }
}
