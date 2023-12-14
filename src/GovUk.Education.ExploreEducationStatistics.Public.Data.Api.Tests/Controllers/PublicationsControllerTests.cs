using FluentAssertions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.DataFixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public class PublicationsControllerTests : IntegrationTestFixture
{
    public PublicationsControllerTests(TestApplicationFactory testApp) : base(testApp)
    {
    }

    [Fact]
    public async Task ListPublications_MultiplePublishedDataSets_Returns200WithAllPublications()
    {
        var dataSet1 = DataFixture
            .DefaultDataSet()
            .WithStatus(DataSetStatus.Published)
            .Generate();

        var dataSet2 = DataFixture
            .DefaultDataSet()
            .WithStatus(DataSetStatus.Published)
            .Generate();

        var dataSet3 = DataFixture
            .DefaultDataSet()
            .WithStatus(DataSetStatus.Unpublished)
            .Generate();

        await AddTestDataSets(dataSet1, dataSet2, dataSet3);

        int page = 1;
        int pageSize = 2;
        string? search = null;

        var contentApiResults = new List<PublicationSearchResultViewModel>
        {
            new()
            {
                Id = dataSet1.PublicationId
            },
            new()
            {
                Id = dataSet2.PublicationId
            }
        };

        var contentApiPaging = new Common.ViewModels.PagingViewModel(page, pageSize, 3);

        var contentApiResponse = new Common.ViewModels.PaginatedListViewModel<PublicationSearchResultViewModel>(contentApiResults, contentApiPaging);

        var publishedDataSetPublicationIds = new List<Guid> { dataSet1.PublicationId, dataSet2.PublicationId };

        ContentApiClientMock
            .Setup(c => c.ListPublications(page, pageSize, search, publishedDataSetPublicationIds))
            .ReturnsAsync(contentApiResponse);

        var query = new Dictionary<string, string?>
        {
            { "page", page.ToString() },
            { "pageSize", pageSize.ToString() },
            { "search", search },
        };

        var uri = QueryHelpers.AddQueryString(_baseUrl, query);

        var response = await TestAppClient.GetAsync(uri);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadFromJsonAsync<PaginatedListViewModel<PublicationListViewModel>>();

        ContentApiClientMock
            .Verify(c => c.ListPublications(page, pageSize, search, publishedDataSetPublicationIds), Times.Once);

        content.Should().NotBeNull();
        content!.Paging.Page.Should().Be(page);
        content.Paging.PageSize.Should().Be(pageSize);
        content.Paging.TotalPages.Should().Be(2);
        content.Paging.TotalResults.Should().Be(3);
        content.Results.Should().HaveCount(contentApiResults.Count);
        content.Results.Should()
            .Contain(r => r.Id == dataSet1.PublicationId)
            .And
            .Contain(r => r.Id == dataSet2.PublicationId);
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

    private const string _baseUrl = "/api/v1/publications";
}
