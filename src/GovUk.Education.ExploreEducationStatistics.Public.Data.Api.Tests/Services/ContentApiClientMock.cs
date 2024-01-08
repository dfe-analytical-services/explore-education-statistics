using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.DataFixtures;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

internal class ContentApiClientMock : IContentApiClient
{
    private readonly DataFixture DataFixture = new();

    public async Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        int page,
        int pageSize,
        string? search = null,
        IEnumerable<Guid>? publicationIds = null)
    {
        if (publicationIds is null)
        {
            return await AllPublications(page, pageSize);
        }

        if (!publicationIds!.Any())
        {
            return await EmptyResult(page, pageSize);
        }

        var publicationsToReturn = PaginatePublications(page, pageSize, publicationIds);

        return await Task.FromResult(new PaginatedListViewModel<PublicationSearchResultViewModel>(
            publicationsToReturn,
            publicationIds!.Count(), 
            page, 
            pageSize));
    }

    private async Task<PaginatedListViewModel<PublicationSearchResultViewModel>> AllPublications(int page, int pageSize)
    {
        var allPublicationIds = GenerateRandomPublicationIds();

        var publicationsToReturn = PaginatePublications(page, pageSize, allPublicationIds);

        return await Task.FromResult(new PaginatedListViewModel<PublicationSearchResultViewModel>(
            publicationsToReturn,
            allPublicationIds.Count, 
            page, 
            pageSize));
    }

    private static IReadOnlyList<Guid> GenerateRandomPublicationIds()
    {
        return Enumerable.Range(0, 50)
            .Select(_ => Guid.NewGuid())
            .ToList();
    }

    private List<PublicationSearchResultViewModel> PaginatePublications(int page, int pageSize,
        IEnumerable<Guid> allPublicationIds)
    {
        return allPublicationIds!
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(CreatePublication)
            .ToList();
    }

    private PublicationSearchResultViewModel CreatePublication(Guid guid)
    {
        return DataFixture
            .Generator<PublicationSearchResultViewModel>()
            .WithDefaults()
            .WithPublicationId(guid)
            .Generate();
    }

    private static async Task<PaginatedListViewModel<PublicationSearchResultViewModel>> EmptyResult(int page,
        int pageSize)
    {
        return await Task.FromResult(new PaginatedListViewModel<PublicationSearchResultViewModel>([], 0, page,
            pageSize));
    }
}
