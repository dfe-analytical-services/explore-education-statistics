using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.DataFixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Net.Http.Json;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

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
        var client = SetupApp(new ContentApiClientMock()).CreateClient();

        var publishedDataSets = GeneratePublishedDataSets(numberOfPublishedDataSets);
        var unpublishedDataSets = GenerateUnublishedDataSets(numberOfUnpublishedDataSets);

        var allDataSets = publishedDataSets.Concat(unpublishedDataSets);

        await AddTestDataSets(allDataSets.ToArray());

        var response = await ListPublications(client, page, pageSize);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<PaginatedListViewModel<PublicationListViewModel>>();

        Assert.NotNull(content);
        Assert.Equal(page, content!.Paging.Page);
        Assert.Equal(pageSize, content!.Paging.PageSize);
        Assert.Equal(numberOfPublishedDataSets, content!.Paging.TotalResults);

        var publishedDataSetPublicationIdsToBeReturned = allDataSets
            .Where(ds => ds.Status == DataSetStatus.Published)
            .Select(ds => ds.PublicationId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        Assert.Equal(publishedDataSetPublicationIdsToBeReturned.Count(), content.Results.Count);

        var publicationIdsThatShouldNotHaveBeenReturned = allDataSets
            .Select(ds => ds.PublicationId)
            .Except(publishedDataSetPublicationIdsToBeReturned);

        foreach (var publicationId in publishedDataSetPublicationIdsToBeReturned)
        {
            Assert.Contains(content.Results, r => r.Id == publicationId);
        }

        foreach (var publicationId in publicationIdsThatShouldNotHaveBeenReturned)
        {
            Assert.DoesNotContain(content.Results, r => r.Id == publicationId);
        }
    }

    [Fact]
    public async Task ListPublications_NoPublishedDataSets_Returns200WithEmptyList()
    {
        var client = SetupApp(new ContentApiClientMock()).CreateClient();

        var response = await ListPublications(client, 1, 1);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<PaginatedListViewModel<PublicationListViewModel>>();

        Assert.NotNull(content);
        Assert.Equal(1, content!.Paging.Page);
        Assert.Equal(1, content!.Paging.PageSize);
        Assert.Equal(0, content!.Paging.TotalResults);
        Assert.Empty(content!.Results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ListPublications_RequestedPageIsTooSmall_Returns400(int page)
    {
        var client = SetupApp(new ContentApiClientMock()).CreateClient();

        var response = await ListPublications(client, page, 1);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(41)]
    public async Task ListPublications_RequestedPageSizeIsOutOfAcceptableRange_Returns400(int pageSize)
    {
        var client = SetupApp(new ContentApiClientMock()).CreateClient();

        var response = await ListPublications(client, 1, pageSize);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
            .Generator<DataSet>()
            .WithDefaults()
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
            .Generator<DataSet>()
            .WithDefaults()
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

    private async Task<HttpResponseMessage> ListPublications(HttpClient client, int page, int pageSize)
    {
        var query = new Dictionary<string, string?>
        {
            { "page", page.ToString() },
            { "pageSize", pageSize.ToString() },
            { "search", null },
        };

        var uri = QueryHelpers.AddQueryString(_baseUrl, query);

        return await client.GetAsync(uri);
    }

    private WebApplicationFactory<Startup> SetupApp(IContentApiClient? contentApiClient = null)
    {
        return TestApp.ConfigureServices(
            services =>
            {
                services.ReplaceService(contentApiClient);
            }
        );
    }

    private const string _baseUrl = "api/v1/publications";
}
