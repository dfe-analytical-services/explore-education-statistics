using Bogus;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;
internal class ContentApiClientMock : IContentApiClient
{
    private readonly Faker faker = new();

    public async Task<PaginatedListViewModel<PublicationSearchResultViewModel>> ListPublications(int page, int pageSize, string? search = null, IEnumerable<Guid>? publicationIds = null)
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

        return await Task.FromResult(new PaginatedListViewModel<PublicationSearchResultViewModel>(publicationsToReturn, publicationIds!.Count(), page, pageSize));
    }

    private async Task<PaginatedListViewModel<PublicationSearchResultViewModel>> AllPublications(int page, int pageSize)
    {
        var allPublicationIds = GenerateRandomPublicationIds();

        var publicationsToReturn = PaginatePublications(page, pageSize, allPublicationIds);

        return await Task.FromResult(new PaginatedListViewModel<PublicationSearchResultViewModel>(publicationsToReturn, allPublicationIds.Count, page, pageSize));
    }

    private static IReadOnlyList<Guid> GenerateRandomPublicationIds()
    {
        return Enumerable.Range(0, 50)
            .Select(_ => Guid.NewGuid())
            .ToList();
    }

    private List<PublicationSearchResultViewModel> PaginatePublications(int page, int pageSize, IEnumerable<Guid> allPublicationIds)
    {
        return allPublicationIds!
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(CreatePublication)
            .ToList();
    }

    private PublicationSearchResultViewModel CreatePublication(Guid guid)
    {
        return new PublicationSearchResultViewModel
        {
            Id = guid,
            Title = faker.Random.String(),
            Slug = faker.Random.String(),
            Summary = faker.Random.String(),
            Theme = faker.Random.String(),
            Published = faker.Date.Past(),
            Type = Content.Model.ReleaseType.OfficialStatistics
        };
    }

    private static async Task<PaginatedListViewModel<PublicationSearchResultViewModel>> EmptyResult(int page, int pageSize)
    {
        return await Task.FromResult(new PaginatedListViewModel<PublicationSearchResultViewModel>([], 0, page, pageSize));
    }
}
