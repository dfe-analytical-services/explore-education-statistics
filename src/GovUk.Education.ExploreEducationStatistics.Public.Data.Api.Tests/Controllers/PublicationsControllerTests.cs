using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.DataFixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public abstract class PublicationsControllerTests : IntegrationTestFixture
{
    private const string BaseUrl = "api/v1/publications";

    public PublicationsControllerTests(TestApplicationFactory testApp) : base(testApp)
    {
    }

    public class ListPublicationsTests : PublicationsControllerTests
    {
        public ListPublicationsTests(TestApplicationFactory testApp) : base(testApp)
        {
        }

        [Theory]
        [InlineData(1, 2, 2, 0)]
        [InlineData(1, 2, 2, 1)]
        [InlineData(2, 2, 2, 1)]
        [InlineData(1, 2, 2, 10)]
        [InlineData(1, 2, 9, 1)]
        [InlineData(2, 2, 9, 1)]
        [InlineData(1, 2, 9, 10)]
        [InlineData(1, 3, 2, 1)]
        public async Task PublishedDataSets_Returns200_FiltersPublicationsWithoutPublishedDataSets(
            int page,
            int pageSize,
            int numberOfPublishedDataSets,
            int numberOfUnpublishedDataSets)
        {
            var client = BuildApp(new ContentApiClientMock()).CreateClient();

            var publishedDataSets = DataFixture
                .DefaultDataSet()
                .WithStatus(DataSetStatus.Published)
                .GenerateList(numberOfPublishedDataSets);

            var unpublishedDataSets = DataFixture
                .DefaultDataSet()
                .WithStatus(DataSetStatus.Unpublished)
                .GenerateList(numberOfUnpublishedDataSets); 

            var allDataSets = publishedDataSets.Concat(unpublishedDataSets).ToArray();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(allDataSets));

            var response = await ListPublications(client, page, pageSize);

            var content = response.AssertOk<PaginatedPublicationListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(page, content.Paging.Page);
            Assert.Equal(pageSize, content.Paging.PageSize);
            Assert.Equal(numberOfPublishedDataSets, content.Paging.TotalResults);

            var expectedPublicationIds = publishedDataSets
                .OrderBy(ds => ds.PublicationId)
                .Select(ds => ds.PublicationId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Assert.Equal(expectedPublicationIds.Count, content.Results.Count);

            var unexpectedPublicationIds = allDataSets
                .Select(ds => ds.PublicationId)
                .Except(expectedPublicationIds);

            foreach (var publicationId in expectedPublicationIds)
            {
                Assert.Contains(content.Results, r => r.Id == publicationId);
            }

            foreach (var publicationId in unexpectedPublicationIds)
            {
                Assert.DoesNotContain(content.Results, r => r.Id == publicationId);
            }
        }

        [Fact]
        public async Task NoPublishedDataSets_Returns200_EmptyList()
        {
            var client = BuildApp(new ContentApiClientMock()).CreateClient();

            var response = await ListPublications(client, 1, 1);

            var content = response.AssertOk<PaginatedPublicationListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(0, content.Paging.TotalResults);
            Assert.Empty(content.Results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task PageTooSmall_Returns400(int page)
        {
            var client = BuildApp(new ContentApiClientMock()).CreateClient();

            var response = await ListPublications(client, page, 1);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasGreaterThanOrEqualError("page");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(41)]
        public async Task PageSizeOutOfBounds_Returns400(int pageSize)
        {
            var client = BuildApp(new ContentApiClientMock()).CreateClient();

            var response = await ListPublications(client, 1, pageSize);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasInclusiveBetweenError("pageSize");
        }

        [Theory]
        [InlineData("a")]
        [InlineData("aa")]
        public async Task SearchTermTooSmall_Returns400(string searchTerm)
        {
            var client = BuildApp(new ContentApiClientMock()).CreateClient();

            var response = await ListPublications(client, null, null, searchTerm);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasMinimumLengthError("search");
        }
    }

    private static async Task<HttpResponseMessage> ListPublications(
        HttpClient client, 
        int? page = null, 
        int? pageSize = null,
        string? search = null)
    {
        var query = new Dictionary<string, string?>
        {
            { "page", page?.ToString() },
            { "pageSize", pageSize?.ToString() },
            { "search", search },
        };

        var uri = QueryHelpers.AddQueryString(BaseUrl, query);

        return await client.GetAsync(uri);
    }

    private WebApplicationFactory<Startup> BuildApp(IContentApiClient? contentApiClient = null)
    {
        return TestApp.ConfigureServices(
            services => { services.ReplaceService(contentApiClient); }
        );
    }
}
