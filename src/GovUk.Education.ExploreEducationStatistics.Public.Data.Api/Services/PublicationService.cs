using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class PublicationService(PublicDataDbContext publicDataDbContext, IContentApiClient contentApiClient) : IPublicationService
{

    public async Task<PaginatedListViewModel<PublicationListViewModel>> ListPublications(int page, int pageSize, string? search = null)
    {
        var publishedDataSetPublicationIds = publicDataDbContext.DataSets
            .Where(ds => ds.Status == DataSetStatus.Published)
            .Select(ds => ds.PublicationId)
            .ToHashSet();

        var paginatedPublications = await contentApiClient.ListPublications(page, pageSize, search, publishedDataSetPublicationIds);

        var results = paginatedPublications.Results.Select(MapPublication).ToList();

        var paging = new PagingViewModel(
            paginatedPublications.Paging.Page, 
            paginatedPublications.Paging.PageSize, 
            paginatedPublications.Paging.TotalResults);

        return new PaginatedListViewModel<PublicationListViewModel>(results, paging);
    }

    private static PublicationListViewModel MapPublication(PublicationSearchResultViewModel publication)
    {
        return new(
            Id: publication.Id,
            Title: publication.Title,
            Slug: publication.Slug);
    }
}
