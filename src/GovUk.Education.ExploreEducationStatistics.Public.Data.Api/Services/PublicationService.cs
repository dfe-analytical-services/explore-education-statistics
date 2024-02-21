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
        string? search = null)
    {
        return await GetPublishedDataSetPublicationIds()
            .OnSuccess(async (publicationIds) =>
                await _contentApiClient.ListPublications(page, pageSize, search, publicationIds))
            .OnSuccess((paginatedPublications) =>
            {
                var results = paginatedPublications.Results.Select(MapPublication).ToList();

                return new PublicationPaginatedListViewModel(
                    results: results,
                    totalResults: paginatedPublications.Paging.TotalResults,
                    page: paginatedPublications.Paging.Page,
                    pageSize: paginatedPublications.Paging.PageSize);
            });
    }

    public async Task<Either<ActionResult, PublicationSummaryViewModel>> GetPublication(Guid publicationId)
    {
        return await CheckPublicationIsPublished(publicationId)
            .OnSuccess(async (publicationIds) => await _contentApiClient.GetPublication(publicationId))
            .OnSuccess(publication => new PublicationSummaryViewModel
            {
                Id  = publication.Id,
                Title = publication.Title,
                Slug = publication.Slug,
                Summary = publication.Summary,
                LastPublished = publication.Published
            });
    }

    private Either<ActionResult, HashSet<Guid>> GetPublishedDataSetPublicationIds()
    {
        return _publicDataDbContext.DataSets
            .Where(ds => ds.Status == DataSetStatus.Published)
            .Select(ds => ds.PublicationId)
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

    private async Task<Either<ActionResult, Unit>> CheckPublicationIsPublished(Guid publicationId)
    {
        var publicationIsPublished = await _publicDataDbContext.DataSets
            .Where(ds => ds.PublicationId == publicationId)
            .AnyAsync(ds => ds.Status == DataSetStatus.Published);

        if (publicationIsPublished)
        {
            return Unit.Instance;
        }

        return new NotFoundResult();
    }
}
