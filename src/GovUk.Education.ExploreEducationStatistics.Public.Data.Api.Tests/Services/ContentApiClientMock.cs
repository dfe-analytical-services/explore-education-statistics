using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

internal class ContentApiClientMock : IContentApiClient
{
    private readonly DataFixture _dataFixture = new();

    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        int page,
        int pageSize,
        string? search = null,
        IEnumerable<Guid>? publicationIds = null)
    {
        if (publicationIds is null)
        {
            return await Task.FromResult(AllPublications(page: page, pageSize: pageSize));
        }

        var publicationIdsList = publicationIds.ToList();

        if (!publicationIdsList.Any())
        {
            return await Task.FromResult(EmptyResult(page: page, pageSize: pageSize));
        }

        var publicationsToReturn = PaginatePublications(
            page: page, 
            pageSize: pageSize, 
            allPublicationIds: publicationIdsList);

        return await Task.FromResult(new PaginatedListViewModel<PublicationSearchResultViewModel>(
            results: publicationsToReturn,
            totalResults: publicationIdsList.Count, 
            page: page, 
            pageSize: pageSize));
    }

    private PaginatedListViewModel<PublicationSearchResultViewModel> AllPublications(int page, int pageSize)
    {
        var allPublicationIds = GenerateRandomPublicationIds();

        var publicationsToReturn = PaginatePublications(
            page: page, 
            pageSize: pageSize, 
            allPublicationIds: allPublicationIds);

        return new PaginatedListViewModel<PublicationSearchResultViewModel>(
            results: publicationsToReturn,
            totalResults: allPublicationIds.Count,
            page: page,
            pageSize: pageSize);
    }

    private static IReadOnlyList<Guid> GenerateRandomPublicationIds()
    {
        return Enumerable.Range(0, 50)
            .Select(_ => Guid.NewGuid())
            .ToList();
    }

    private List<PublicationSearchResultViewModel> PaginatePublications(
        int page,
        int pageSize,
        IEnumerable<Guid> allPublicationIds)
    {
        return allPublicationIds
            .Order()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(CreatePublication)
            .ToList();
    }

    private PublicationSearchResultViewModel CreatePublication(Guid guid)
    {
        return _dataFixture
           .Generator<PublicationSearchResultViewModel>()
           .ForInstance(s => s
               .SetDefault(p => p.Id)
               .SetDefault(p => p.Title)
               .SetDefault(p => p.Slug)
               .SetDefault(p => p.Summary)
               .SetDefault(p => p.Theme)
               .Set(p => p.Published, p => p.Date.Past())
               .Set(p => p.Type, Content.Model.ReleaseType.OfficialStatistics)
               .Set(p => p.Id, guid))
           .Generate();
    }

    private static PaginatedListViewModel<PublicationSearchResultViewModel> EmptyResult(int page,
        int pageSize)
    {
        return new PaginatedListViewModel<PublicationSearchResultViewModel>([], 0, page, pageSize);
    }
}
