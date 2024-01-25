using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.DataFixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Moq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

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

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(41)]
        public async Task PageSizeOutOfBounds_Returns400(int pageSize)
        {
            var client = BuildApp(new ContentApiClientMock()).CreateClient();

            var response = await ListPublications(client, 1, pageSize);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("aa")]
        public async Task SearchTermTooSmall_Returns400(string searchTerm)
        {
            var client = BuildApp(new ContentApiClientMock()).CreateClient();

            var response = await ListPublications(client, null, null, searchTerm);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    public class GetPublicationTests : PublicationsControllerTests
    {
        public GetPublicationTests(TestApplicationFactory testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task PublicationExists_Returns200()
        {
            var publication = DataFixture
                .Generator<PublicationCacheViewModel>()
                .ForInstance(s => s
                    .SetDefault(f => f.Id)
                    .SetDefault(f => f.Title)
                    .SetDefault(f => f.Slug)
                    .SetDefault(f => f.Summary)
                    .Set(f => f.Published, f => f.Date.Past())
                    .SetDefault(f => f.LatestReleaseId))
                .Generate();

            var contentApiClient = new Mock<IContentApiClient>();
            contentApiClient
                .Setup(c => c.GetPublication(It.IsAny<Guid>()))
                .ReturnsAsync(publication);

            var client = BuildApp(contentApiClient.Object).CreateClient();

            var publishedDataSet = GeneratePublishedDataSet(publication.Id);
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(publishedDataSet));

            var response = await GetPublication(client, publication.Id);

            var content = response.AssertOk<ViewModels.PublicationSummaryViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(publication.Id, content.Id);
            Assert.Equal(publication.Title, content.Title);
            Assert.Equal(publication.Slug, content.Slug);
            Assert.Equal(publication.Summary, content.Summary);
            Assert.Equal(publication.Published, content.LastPublished);
        }

        [Fact]
        public async Task PublicationDoesNotExist_Returns404()
        {
            var publicationId = Guid.NewGuid();

            var publishedDataSet = GeneratePublishedDataSet(publicationId);
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(publishedDataSet));

            var contentApiClient = new Mock<IContentApiClient>();
            contentApiClient
                .Setup(c => c.GetPublication(publicationId))
                .ReturnsAsync(new NotFoundObjectResult(new ProblemDetails { Detail = "not found deails" }));
            
            var client = BuildApp(contentApiClient.Object).CreateClient();

            var response = await GetPublication(client, publicationId);

            response.AssertNotFound(new ProblemDetails { Detail = "not found deails", Status = (int?)HttpStatusCode.NotFound }, true);
        }

        [Fact]
        public async Task NotPublished_Returns404()
        {
            var client = BuildApp().CreateClient();

            var response = await GetPublication(client, It.IsAny<Guid>());

            response.AssertNotFound();
        }

        [Fact]
        public async Task ContentApiThrowsException_Returns500()
        {
            var publicationId = Guid.NewGuid();

            var publishedDataSet = GeneratePublishedDataSet(publicationId);
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(publishedDataSet));

            var contentApiClient = new Mock<IContentApiClient>();
            contentApiClient
                .Setup(c => c.GetPublication(publicationId))
                .ThrowsAsync(new HttpRequestException("something went wrong"));

            var client = BuildApp(contentApiClient.Object).CreateClient();
            var response = await GetPublication(client, publicationId);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.Contains("something went wrong", await response.Content.ReadAsStringAsync());
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

    private static async Task<HttpResponseMessage> GetPublication(HttpClient client, Guid publicationId)
    {
        var uri = new Uri($"{BaseUrl}/{publicationId}", UriKind.Relative);

        return await client.GetAsync(uri);
    }

    private DataSet GeneratePublishedDataSet(Guid publicationId)
    {
        return DataFixture
            .DefaultDataSet()
            .WithStatus(DataSetStatus.Published)
            .WithPublicationId(publicationId)
            .Generate();
    }

    private WebApplicationFactory<Startup> BuildApp(IContentApiClient? contentApiClient = null)
    {
        return TestApp.ConfigureServices(
            services => { services.ReplaceService(contentApiClient ?? Mock.Of<IContentApiClient>()); }
        );
    }
}
