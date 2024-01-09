using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;
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
        this._publicDataDbContext = publicDataDbContext;
        this._contentApiClient = contentApiClient;
    }

    public async Task<Either<ActionResult, PaginatedPublicationListViewModel>> ListPublications(
        int page,
        int pageSize, 
        string? search = null)
    {
        return await GetPublishedDataSetPublicationIds(_publicDataDbContext)
            .OnSuccess(async (publicationIds) =>
                await _contentApiClient.ListPublications(page, pageSize, search, publicationIds))
            .OnSuccess((paginatedPublications) =>
            {
                var results = paginatedPublications.Results.Select(MapPublication).ToList();

                return new PaginatedPublicationListViewModel(
                    Results: results, 
                    TotalResults: paginatedPublications.Paging.TotalResults,
                    Page: paginatedPublications.Paging.Page,
                    PageSize: paginatedPublications.Paging.PageSize
                    );
            });
    }

    private static Either<ActionResult, HashSet<Guid>> GetPublishedDataSetPublicationIds(
        PublicDataDbContext publicDataDbContext)
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
