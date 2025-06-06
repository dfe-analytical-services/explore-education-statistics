using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicationSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.PublicationSummaryViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class PublicationService(
    PublicDataDbContext publicDataDbContext,
    IContentApiClient contentApiClient,
    IAnalyticsService analyticsService)
    : IPublicationService
{
    public async Task<Either<ActionResult, PublicationPaginatedListViewModel>> ListPublications(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        return await
            GetPublishedDataSetPublicationIds(cancellationToken)
            .OnSuccess(async publicationIds =>
                await contentApiClient.ListPublications(
                    page: page, 
                    pageSize: pageSize, 
                    search:search, 
                    publicationIds: publicationIds,
                    cancellationToken: cancellationToken))
            .OnSuccess(paginatedPublications =>
            {
                var results = paginatedPublications
                    .Results
                    .Select(MapPublication)
                    .ToList();

                return new PublicationPaginatedListViewModel
                {
                    Results = results,
                    Paging = new PagingViewModel(
                        totalResults: paginatedPublications.Paging.TotalResults,
                        page: paginatedPublications.Paging.Page,
                        pageSize: paginatedPublications.Paging.PageSize),
                };
            })
            .OnSuccessDo(() => analyticsService.CaptureTopLevelCall(
                type: TopLevelCallType.GetPublications,
                parameters: new PaginationParameters(Page: page, PageSize: pageSize),
                cancellationToken: cancellationToken));
    }

    public async Task<Either<ActionResult, PublicationSummaryViewModel>> GetPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return await CheckPublicationIsPublished(publicationId, cancellationToken)
            .OnSuccess(async _ => await contentApiClient.GetPublication(publicationId, cancellationToken))
            .OnSuccess(publication => new PublicationSummaryViewModel
            {
                Id  = publication.Id,
                Title = publication.Title,
                Slug = publication.Slug,
                Summary = publication.Summary,
                LastPublished = publication.Published
            });
    }

    private async Task<Either<ActionResult, HashSet<Guid>>> GetPublishedDataSetPublicationIds(
        CancellationToken cancellationToken = default)
    {
        return (await publicDataDbContext.DataSets
                .Where(ds => ds.Status == DataSetStatus.Published)
                .Select(ds => ds.PublicationId)
                .ToListAsync(cancellationToken: cancellationToken)
            )
            .ToHashSet();
    }

    private static PublicationSummaryViewModel MapPublication(PublicationSearchResultViewModel publication)
    {
        return new PublicationSummaryViewModel
        {
            Id = publication.Id,
            Title = publication.Title,
            Slug = publication.Slug,
            Summary = publication.Summary,
            LastPublished = publication.Published
        };
    }

    private async Task<Either<ActionResult, Unit>> CheckPublicationIsPublished(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        var publicationIsPublished = await publicDataDbContext.DataSets
            .Where(ds => ds.PublicationId == publicationId)
            .AnyAsync(ds => ds.Status == DataSetStatus.Published, cancellationToken: cancellationToken);

        if (publicationIsPublished)
        {
            return Unit.Instance;
        }

        return new NotFoundResult();
    }
}
