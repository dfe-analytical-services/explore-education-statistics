using System.Net;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Moq;
using PublicationSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.PublicationSummaryViewModel;
using System.Drawing.Printing;

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

            var publications = new List<PublicationSearchResultViewModel>();

            var publishedDataSetPublicationIds = publishedDataSets
                .Select(p => p.PublicationId)
                .ToList();

            var lastPageNumber = (int)Math.Ceiling((decimal)numberOfPublishedDataSets / pageSize);

            var numberOfPublicationsToReturn = page > lastPageNumber 
                ? 0
                : page < lastPageNumber 
                ? pageSize
                : numberOfPublishedDataSets % pageSize;

            var publicationIdsToReturn = publishedDataSetPublicationIds
                .Take(numberOfPublicationsToReturn)
                .ToList();

            foreach (var publicationId in publicationIdsToReturn)
            {
                var publication = DataFixture
                    .Generator<PublicationSearchResultViewModel>()
                    .ForInstance(s => s
                        .SetDefault(p => p.Title)
                        .SetDefault(p => p.Slug)
                        .SetDefault(p => p.Summary)
                        .SetDefault(p => p.Theme)
                        .Set(p => p.Published, p => p.Date.Past())
                        .Set(p => p.Type, ReleaseType.OfficialStatistics)
                        .Set(p => p.Id, publicationId))
                    .Generate();

                publications.Add(publication);
            }

            var contentApiClient = new Mock<IContentApiClient>();
            contentApiClient
                .Setup(c => c.ListPublications(
                    page,
                    pageSize,
                    null,
                    It.Is<IEnumerable<Guid>>(ids => 
                        Enumerable.SequenceEqual(
                            ids.Order(),
                            publishedDataSetPublicationIds.Order()))))
                .ReturnsAsync(new Common.ViewModels.PaginatedListViewModel<PublicationSearchResultViewModel>(
                    results: publications,
                    totalResults: numberOfPublishedDataSets,
                    page: page,
                    pageSize: pageSize));

            var client = BuildApp(contentApiClient.Object).CreateClient();

            var response = await ListPublications(client, page, pageSize);

            var content = response.AssertOk<PaginatedPublicationListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(page, content.Paging.Page);
            Assert.Equal(pageSize, content.Paging.PageSize);
            Assert.Equal(numberOfPublishedDataSets, content.Paging.TotalResults);
            Assert.Equal(publicationIdsToReturn.Count, content.Results.Count);

            var unexpectedPublicationIds = allDataSets
                .Select(ds => ds.PublicationId)
                .Except(publicationIdsToReturn);

            foreach (var publicationId in publicationIdsToReturn)
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
            var contentApiClient = new Mock<IContentApiClient>();
            contentApiClient
                .Setup(c => c.ListPublications(
                    1,
                    1,
                    null,
                    Enumerable.Empty<Guid>()))
                .ReturnsAsync(new Common.ViewModels.PaginatedListViewModel<PublicationSearchResultViewModel>(
                    results: [],
                    totalResults: 0,
                    page: 1,
                    pageSize: 1));

            var client = BuildApp(contentApiClient.Object).CreateClient();

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
            var client = BuildApp().CreateClient();

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
            var client = BuildApp().CreateClient();

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
            var client = BuildApp().CreateClient();

            var response = await ListPublications(client, null, null, searchTerm);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasMinimumLengthError("search");
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
                .Generator<PublishedPublicationSummaryViewModel>()
                .ForInstance(s => s
                    .SetDefault(f => f.Id)
                    .SetDefault(f => f.Title)
                    .SetDefault(f => f.Slug)
                    .SetDefault(f => f.Summary)
                    .Set(f => f.Published, f => f.Date.Past()))
                .Generate();

            var contentApiClient = new Mock<IContentApiClient>();
            contentApiClient
                .Setup(c => c.GetPublication(publication.Id))
                .ReturnsAsync(publication);

            var client = BuildApp(contentApiClient.Object).CreateClient();

            var publishedDataSet = GeneratePublishedDataSet(publication.Id);
            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(publishedDataSet));

            var response = await GetPublication(client, publication.Id);

            var content = response.AssertOk<PublicationSummaryViewModel>(useSystemJson: true);

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
                .ReturnsAsync(new NotFoundResult());
            
            var client = BuildApp(contentApiClient.Object).CreateClient();

            var response = await GetPublication(client, publicationId);

            response.AssertNotFound();
        }

        [Fact]
        public async Task NotDataSets_Returns404()
        {
            var client = BuildApp().CreateClient();

            var response = await GetPublication(client, It.IsAny<Guid>());

            response.AssertNotFound();
        }

        [Fact]
        public async Task UnpublishedDataSets_Returns404()
        {
            var publicationId = Guid.NewGuid();

            var unpublishedDataSets = DataFixture
                .DefaultDataSet()
                .WithStatus(DataSetStatus.Unpublished)
                .WithPublicationId(publicationId)
                .GenerateList(3);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(unpublishedDataSets));

            var client = BuildApp().CreateClient();

            var response = await GetPublication(client, publicationId);

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

        private DataSet GeneratePublishedDataSet(Guid publicationId)
        {
            return DataFixture
                .DefaultDataSet()
                .WithStatus(DataSetStatus.Published)
                .WithPublicationId(publicationId)
                .Generate();
        }

        private static async Task<HttpResponseMessage> GetPublication(HttpClient client, Guid publicationId)
        {
            var uri = new Uri($"{BaseUrl}/{publicationId}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class ListDataSetsTests : PublicationsControllerTests
    {
        public ListDataSetsTests(TestApplicationFactory testApp) : base(testApp)
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
        public async Task PublishedDataSetsForRequestedPublication_Returns200(
             int page,
             int pageSize,
             int numberOfPublishedDataSets,
             int numberOfUnpublishedDataSets)
        {
            var publicationId = Guid.NewGuid();

            var publishedDataSets = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publicationId)
                .GenerateList(numberOfPublishedDataSets);

            var unpublishedDataSets = DataFixture
                .DefaultDataSet()
                .WithStatusUnpublished()
                .WithPublicationId(publicationId)
                .GenerateList(numberOfUnpublishedDataSets);

            var allDataSets = publishedDataSets.Concat(unpublishedDataSets).ToArray();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(allDataSets));

            var allDataSetVersions = allDataSets
                .Select(ds =>
                    DataFixture
                    .DefaultDataSetVersion(1, 1, 1, 3)
                    .WithStatusPublished()
                    .WithDataSetId(ds.Id)
                    .Generate())
                .ToList();

            var dataSetVersionsLookupByDataSetId = allDataSetVersions
                .ToDictionary(dsv => dsv.DataSetId);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(allDataSetVersions);

                foreach (var dataSet in context.DataSets)
                {
                    dataSet.LatestVersionId = dataSetVersionsLookupByDataSetId[dataSet.Id].Id;
                }
            });

            var dataSetIdsToReturn = publishedDataSets
                .OrderByDescending(ds => ds.LatestVersion!.Published)
                .ThenBy(ds => ds.Title)
                .ThenBy(ds => ds.Id) // The service returns the DataSets ordered by Published date (DESC), then by Title, then by ID
                .Select(ds => ds.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client,
                publicationId: publicationId,
                page: page,
                pageSize: pageSize);

            var content = response.AssertOk<PaginatedDataSetViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(page, content.Paging.Page);
            Assert.Equal(pageSize, content.Paging.PageSize);
            Assert.Equal(numberOfPublishedDataSets, content.Paging.TotalResults);
            Assert.Equal(dataSetIdsToReturn.Count, content.Results.Count);

            var unexpectedDataSetIds = allDataSets
                .Select(ds => ds.Id)
                .Except(dataSetIdsToReturn);

            foreach (var id in dataSetIdsToReturn)
            {
                Assert.Contains(content.Results, r => r.Id == id);
            }

            foreach (var id in unexpectedDataSetIds)
            {
                Assert.DoesNotContain(content.Results, r => r.Id == id);
            }
        }

        [Fact]
        public async Task NoPublishedDataSetsForRequestedPublication_PublishedDataSetsForOtherPublication_Returns200_EmptyList()
        {
            var publicationIdWithPublishedDataSets = Guid.NewGuid();

            var publishedDataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(DataSetStatus.Published)
                .WithPublicationId(publicationIdWithPublishedDataSets)
                .Generate();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(publishedDataSet));

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client,
                publicationId: Guid.NewGuid(),
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<PaginatedDataSetViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(0, content.Paging.TotalResults);
            Assert.Empty(content.Results);
        }

        [Fact]
        public async Task NoPublishedDataSets_Returns200_EmptyList()
        {
            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client,
                publicationId: Guid.NewGuid(),
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<PaginatedDataSetViewModel>(useSystemJson: true);

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
            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client,
                publicationId: Guid.NewGuid(),
                page: page,
                pageSize: 1);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasGreaterThanOrEqualError("page");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(21)]
        public async Task PageSizeOutOfBounds_Returns400(int pageSize)
        {
            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client, 
                publicationId: Guid.NewGuid(), 
                page: 1, 
                pageSize: pageSize);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasInclusiveBetweenError("pageSize");
        }

        [Fact]
        public async Task InvalidPublicationId_Returns404()
        {
            var client = BuildApp().CreateClient();

            var query = new Dictionary<string, string?>
            {
                { "page", "1" },
                { "pageSize", "1" },
            };

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/not_a_valid_guid/data-sets", query);

            var response = await client.GetAsync(uri);

            response.AssertNotFound();
        }

        private static async Task<HttpResponseMessage> ListDataSets(
            HttpClient client,
            Guid publicationId,
            int? page = null,
            int? pageSize = null)
        {
            var query = new Dictionary<string, string?>
            {
                { "page", page?.ToString() },
                { "pageSize", pageSize?.ToString() },
            };

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{publicationId}/data-sets", query);

            return await client.GetAsync(uri);
        }
    }

    private WebApplicationFactory<Startup> BuildApp(IContentApiClient? contentApiClient = null)
    {
        return TestApp.ConfigureServices(
            services => { services.ReplaceService(contentApiClient ?? Mock.Of<IContentApiClient>()); }
        );
    }
}
