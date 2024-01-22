using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;

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

    public async Task<Either<ActionResult, PaginatedPublicationListViewModel>> ListPublications(
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

                return new PaginatedPublicationListViewModel(
                    results: results,
                    totalResults: paginatedPublications.Paging.TotalResults,
                    page: paginatedPublications.Paging.Page,
                    pageSize: paginatedPublications.Paging.PageSize);
            });
    }

    private Either<ActionResult, HashSet<Guid>> GetPublishedDataSetPublicationIds()
    {
        return _publicDataDbContext.DataSets
            .Where(ds => ds.Status == DataSetStatus.Published)
            .Select(ds => ds.PublicationId)
            .ToHashSet();
    }

    private static PublicationListViewModel MapPublication(PublicationSearchResultViewModel publication)
    {
        return new PublicationListViewModel 
        {
            Id = publication.Id,
            Title = publication.Title,
            Slug = publication.Slug,
            Summary = publication.Summary,
            LastPublished = publication.Published
        };
    }
}
