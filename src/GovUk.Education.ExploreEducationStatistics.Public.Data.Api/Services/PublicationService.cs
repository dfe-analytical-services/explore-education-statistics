using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class PublicationService(PublicDataDbContext publicDataDbContext, IContentApiClient contentApiClient) : IPublicationService
{

    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationListViewModel>>> ListPublications(int page, int pageSize, string? search = null)
    {
        return await GetPublishedDataSetPublicationIds(publicDataDbContext)
            .OnSuccess(async (publicationIds) => await contentApiClient.ListPublications(page, pageSize, search, publicationIds))
            .OnSuccess((paginatedPublications) =>
            {
                var results = paginatedPublications.Results.Select(MapPublication).ToList();

                var paging = new PagingViewModel(
                    paginatedPublications.Paging.Page,
                    paginatedPublications.Paging.PageSize,
                    paginatedPublications.Paging.TotalResults);

                return new PaginatedListViewModel<PublicationListViewModel>(results, paging);
            });
    }

    private static Either<ActionResult, HashSet<Guid>> GetPublishedDataSetPublicationIds(PublicDataDbContext publicDataDbContext)
    {
        return publicDataDbContext.DataSets
            .Where(ds => ds.Status == DataSetStatus.Published)
            .Select(ds => ds.PublicationId)
            .ToHashSet();
    }

    private static PublicationListViewModel MapPublication(PublicationSearchResultViewModel publication)
    {
        return new(
            Id: publication.Id,
            Title: publication.Title,
            Slug: publication.Slug);
    }
}
