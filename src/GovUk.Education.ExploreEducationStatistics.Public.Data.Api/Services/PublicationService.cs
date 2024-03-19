using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicationSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.PublicationSummaryViewModel;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class PublicationService : IPublicationService
{
    private readonly PublicDataDbContext _publicDataDbContext;
    private readonly IContentApiClient _contentApiClient;

    public PublicationService(PublicDataDbContext publicDataDbContext, IContentApiClient contentApiClient)
    {
        _publicDataDbContext = publicDataDbContext;
        _contentApiClient = contentApiClient;
    }

    public async Task<Either<ActionResult, PublicationPaginatedListViewModel>> ListPublications(
        int page,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        return await GetPublishedDataSetPublicationIds()
            .OnSuccess(async publicationIds =>
                await _contentApiClient.ListPublications(
                    page: page, 
                    pageSize: pageSize, 
                    search:search, 
                    publicationIds: publicationIds,
                    cancellationToken: cancellationToken))
            .OnSuccess(paginatedPublications =>
            {
                var results = paginatedPublications.Results.Select(MapPublication).ToList();

                return new PublicationPaginatedListViewModel
                {
                    Results = results,
                    Paging = new PagingViewModel(
                        totalResults: paginatedPublications.Paging.TotalResults,
                        page: paginatedPublications.Paging.Page,
                        pageSize: paginatedPublications.Paging.PageSize),
                };
            });
    }

    public async Task<Either<ActionResult, PublicationSummaryViewModel>> GetPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default)
    {
        return await CheckPublicationIsPublished(publicationId, cancellationToken)
            .OnSuccess(async _ => await _contentApiClient.GetPublication(publicationId, cancellationToken))
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
        return (await _publicDataDbContext.DataSets
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
        var publicationIsPublished = await _publicDataDbContext.DataSets
            .Where(ds => ds.PublicationId == publicationId)
            .AnyAsync(ds => ds.Status == DataSetStatus.Published, cancellationToken: cancellationToken);

        if (publicationIsPublished)
        {
            return Unit.Instance;
        }

        return new NotFoundResult();
    }
}
