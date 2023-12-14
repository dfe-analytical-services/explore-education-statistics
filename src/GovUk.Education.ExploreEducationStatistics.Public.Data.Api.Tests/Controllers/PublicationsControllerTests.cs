using FluentAssertions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.DataFixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Net.Http.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public class PublicationsControllerTests : IntegrationTestFixture
{
    public PublicationsControllerTests(TestApplicationFactory testApp) : base(testApp)
    {
    }

    [Theory]
    [InlineData(1, 2, 2, 1)]
    [InlineData(2, 2, 2, 1)]
    [InlineData(1, 2, 2, 10)]
    [InlineData(1, 2, 9, 1)]
    [InlineData(2, 2, 9, 1)]
    [InlineData(1, 2, 9, 10)]
    [InlineData(1, 3, 2, 1)]
    public async Task ListPublications_MultiplePublishedDataSets_Returns200WithAllPublicationsForPublishedDataSets(
        int page,
        int pageSize,
        int numberOfPublishedDataSets,
        int numberOfUnpublishedDataSets)
    {
        var publishedDataSets = GeneratePublishedDataSets(numberOfPublishedDataSets);
        var unpublishedDataSets = GenerateUnublishedDataSets(numberOfUnpublishedDataSets);

        var allDataSets = publishedDataSets.Concat(unpublishedDataSets);

        await AddTestDataSets(allDataSets.ToArray());

        var response = await ListPublications(page, pageSize);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<PaginatedListViewModel<PublicationListViewModel>>();

        content.Should().NotBeNull();
        content!.Paging.Page.Should().Be(page);
        content.Paging.PageSize.Should().Be(pageSize);
        content.Paging.TotalResults.Should().Be(numberOfPublishedDataSets);

        var publishedDataSetPublicationIdsToBeReturned = allDataSets
            .Where(ds => ds.Status == DataSetStatus.Published)
            .Select(ds => ds.PublicationId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        content.Results.Should().HaveCount(publishedDataSetPublicationIdsToBeReturned.Count());

        var publicationIdsThatShouldNotHaveBeenReturned = allDataSets
            .Select(ds => ds.PublicationId)
            .Except(publishedDataSetPublicationIdsToBeReturned);

        foreach (var publicationId in publishedDataSetPublicationIdsToBeReturned)
        {
            content.Results.Should().Contain(r => r.Id == publicationId);
        }

        foreach (var publicationId in publicationIdsThatShouldNotHaveBeenReturned)
        {
            content.Results.Should().NotContain(r => r.Id == publicationId);
        }
    }

    private IReadOnlyList<DataSet> GeneratePublishedDataSets(int numberToGenerate)
    {
        return Enumerable.Range(0, numberToGenerate)
            .Select(_ => GeneratePublishedDataSet())
            .ToList();
    }

    private DataSet GeneratePublishedDataSet()
    {
        return DataFixture
            .DefaultDataSet()
            .WithStatus(DataSetStatus.Published)
            .Generate();
    }

    private IReadOnlyList<DataSet> GenerateUnublishedDataSets(int numberToGenerate)
    {
        return Enumerable.Range(0, numberToGenerate)
            .Select(_ => GenerateUnublishedDataSet())
            .ToList();
    }

    private DataSet GenerateUnublishedDataSet()
    {
        return DataFixture
            .DefaultDataSet()
            .WithStatus(DataSetStatus.Unpublished)
            .Generate();
    }

    private async Task AddTestDataSets(params DataSet[] dataSets)
    {
        foreach (var dataSet in dataSets)
        {
            await AddTestDataSet(dataSet);
        }
    }

    private async Task AddTestDataSet(DataSet dataSet)
    {
        async Task supplier(PublicDataDbContext dbContext) => await dbContext.AddAsync(dataSet);

        await TestApp.AddTestData<PublicDataDbContext>(supplier);
    }

    private async Task<HttpResponseMessage> ListPublications(int page, int pageSize)
    {
        var query = new Dictionary<string, string?>
        {
            { "page", page.ToString() },
            { "pageSize", pageSize.ToString() },
            { "search", null },
        };

        var uri = QueryHelpers.AddQueryString(_baseUrl, query);

        return await TestAppClient.GetAsync(uri);
    }

    private const string _baseUrl = "api/v1/publications";
}
