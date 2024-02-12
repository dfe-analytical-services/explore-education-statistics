using System.Net;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Documents.SystemFunctions;
using Moq;
using PublicationSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.PublicationSummaryViewModel;

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

        [Fact]
        public async Task PublishedDataSets_Returns200_FiltersPublicationsWithoutPublishedDataSets()
        {
            var page = 1;
            var pageSize = 2;
            var numberOfPublishedDataSets = 3;
            var numberOfUnpublishedDataSets = 1;

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

            var publishedDataSetPublicationIds = publishedDataSets
                .Select(p => p.PublicationId)
                .ToList();

            var expectedPublicationIds = publishedDataSetPublicationIds
                .Take(2)
                .ToList();

            var expectedPublications = expectedPublicationIds
                .Select(id =>
                    DataFixture
                    .Generator<PublicationSearchResultViewModel>()
                    .ForInstance(s => s
                        .SetDefault(p => p.Title)
                        .SetDefault(p => p.Slug)
                        .SetDefault(p => p.Summary)
                        .SetDefault(p => p.Theme)
                        .Set(p => p.Published, p => p.Date.Past())
                        .Set(p => p.Type, ReleaseType.OfficialStatistics)
                        .Set(p => p.Id, id))
                    .Generate()
                )
                .ToList();

            var contentApiClient = new Mock<IContentApiClient>();
            contentApiClient
                .Setup(c => c.ListPublications(
                    page,
                    pageSize,
                    null,
                    It.Is<IEnumerable<Guid>>(ids =>
                        ids.Order().SequenceEqual(publishedDataSetPublicationIds.Order()))))
                .ReturnsAsync(new Common.ViewModels.PaginatedListViewModel<PublicationSearchResultViewModel>(
                    results: expectedPublications,
                    totalResults: numberOfPublishedDataSets,
                    page: page,
                    pageSize: pageSize));

            var client = BuildApp(contentApiClient.Object).CreateClient();

            var response = await ListPublications(
                client: client,
                page: page,
                pageSize: pageSize);

            var content = response.AssertOk<PublicationPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(page, content.Paging.Page);
            Assert.Equal(pageSize, content.Paging.PageSize);
            Assert.Equal(numberOfPublishedDataSets, content.Paging.TotalResults);
            Assert.Equal(expectedPublicationIds.Count, content.Results.Count);

            Assert.All(content.Results, r => Assert.Contains(expectedPublicationIds, id => id == r.Id));
        }

        [Fact]
        public async Task ReturnsCorrectViewModel()
        {
            var publicationId = Guid.NewGuid();

            var dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publicationId)
                .Generate();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

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

            var contentApiClient = new Mock<IContentApiClient>();
            contentApiClient
                .Setup(c => c.ListPublications(
                    1,
                    1,
                    null,
                    new List<Guid>() { publicationId }))
                .ReturnsAsync(new Common.ViewModels.PaginatedListViewModel<PublicationSearchResultViewModel>(
                    results: [publication],
                    totalResults: 1,
                    page: 1,
                    pageSize: 1));

            var client = BuildApp(contentApiClient.Object).CreateClient();

            var response = await ListPublications(
                client: client,
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<PublicationPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(1, content.Paging.TotalResults);

            var result = Assert.Single(content.Results);

            Assert.Equal(publication.Id, result.Id);
            Assert.Equal(publication.Title, result.Title);
            Assert.Equal(publication.Slug, result.Slug);
            Assert.Equal(publication.Summary, result.Summary);
            Assert.Equal(publication.Published, result.LastPublished);
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

            var response = await ListPublications(
                client: client,
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<PublicationPaginatedListViewModel>(useSystemJson: true);

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

            var response = await ListPublications(
                client: client,
                page: page,
                pageSize: 1);

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

            var response = await ListPublications(
                client: client,
                page: 1,
                pageSize: pageSize);

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

            var response = await ListPublications(
                client: client,
                page: null,
                pageSize: null,
                search: searchTerm);

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

        public static IEnumerable<object[]> AvailableDataSetPaginationCombinations()
        {
            var dataSetStatuses = new HashSet<DataSetStatus>()
            {
                DataSetStatus.Published,
                DataSetStatus.Deprecated,
            };

            var paginationCombinations = new List<(
                int page, 
                int pageSize, 
                int numberOfAvailableDataSets, 
                int numberOfUnpublishedDataSets)>()
            {
                ( 1, 2, 2, 0 ),
                ( 1, 2, 2, 1 ),
                ( 1, 2, 2, 10 ),
                ( 1, 2, 9, 1 ),
                ( 2, 2, 9, 1 ),
                ( 1, 2, 9, 10 ),
                ( 1, 3, 2, 1 ),
            };

            foreach (var dataSetStatus in dataSetStatuses)
            {
                foreach (var paginationCombination in paginationCombinations)
                {
                    yield return new object[]
                    {
                        dataSetStatus,
                        paginationCombination.page,
                        paginationCombination.pageSize,
                        paginationCombination.numberOfAvailableDataSets,
                        paginationCombination.numberOfUnpublishedDataSets
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(AvailableDataSetPaginationCombinations))]
        public async Task MultipleDataSetsAvailableForRequestedPublication_Returns200(
            DataSetStatus availableDataSetStatus,
            int page,
            int pageSize,
            int numberOfAvailableDataSets,
            int numberOfUnpublishedDataSets)
        {
            var publicationId = Guid.NewGuid();

            var availableDataSets = DataFixture
                .DefaultDataSet()
                .WithStatus(availableDataSetStatus)
                .WithPublicationId(publicationId)
                .GenerateList(numberOfAvailableDataSets);

            var unpublishedDataSets = DataFixture
                .DefaultDataSet()
                .WithStatusUnpublished()
                .WithPublicationId(publicationId)
                .GenerateList(numberOfUnpublishedDataSets);

            var dataSets = availableDataSets.Concat(unpublishedDataSets).ToList();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSets));

            var dataSetVersions = dataSets
                .Select(ds =>
                    DataFixture
                    .DefaultDataSetVersion(
                        filters: 1,
                        indicators: 1,
                        locations: 1,
                        timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSet(ds)
                    .FinishWith(dsv => ds.LatestVersion = dsv)
                    .Generate())
                .ToList();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(dataSetVersions);
                context.DataSets.UpdateRange(dataSets);
            });

            var expectedDataSetIds = availableDataSets
                .OrderByDescending(ds => ds.LatestVersion!.Published)
                .ThenBy(ds => ds.Title)
                .ThenBy(ds => ds.Id)
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

            var content = response.AssertOk<DataSetPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(page, content.Paging.Page);
            Assert.Equal(pageSize, content.Paging.PageSize);
            Assert.Equal(numberOfAvailableDataSets, content.Paging.TotalResults);
            Assert.Equal(expectedDataSetIds.Count, content.Results.Count);

            Assert.All(content.Results, r => Assert.Contains(expectedDataSetIds, id => id == r.Id));
        }

        [Theory]
        [InlineData(DataSetStatus.Published)]
        [InlineData(DataSetStatus.Deprecated)]
        public async Task ReturnsCorrectViewModel(DataSetStatus dataSetStatus)
        {
            var publicationId = Guid.NewGuid();

            var dataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(dataSetStatus)
                .WithPublicationId(publicationId)
                .Generate();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestVersion = dsv)
                .Generate();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client,
                publicationId: publicationId,
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<DataSetPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(1, content.Paging.TotalResults);

            var result = Assert.Single(content.Results);

            Assert.Equal(dataSet.Id, result.Id);
            Assert.Equal(dataSet.Title, result.Title);
            Assert.Equal(dataSet.Summary, result.Summary);
            Assert.Equal(dataSet.Status, result.Status);
            Assert.Equal(dataSet.SupersedingDataSetId, result.SupersedingDataSetId);
            Assert.Equal(dataSetVersion.Version, result.LatestVersion.Number);
            Assert.Equal(
                dataSetVersion.Published!.Value.ToUnixTimeSeconds(),
                result.LatestVersion.Published.ToUnixTimeSeconds()
            );
            Assert.Equal(dataSetVersion.TotalResults, result.LatestVersion.TotalResults);
            Assert.Equal(
                TimePeriodFormatter.Format(
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Year,
                    dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                result.LatestVersion.TimePeriods.Start);
            Assert.Equal(
                TimePeriodFormatter.Format(
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Year,
                    dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                result.LatestVersion.TimePeriods.End);
            Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, result.LatestVersion.GeographicLevels);
            Assert.Equal(dataSetVersion.MetaSummary.Filters, result.LatestVersion.Filters);
            Assert.Equal(dataSetVersion.MetaSummary.Indicators, result.LatestVersion.Indicators);
        }

        [Theory]
        [InlineData(DataSetStatus.Published)]
        [InlineData(DataSetStatus.Deprecated)]
        public async Task DataSetAvailableForOtherPublication_Returns200_OnlyRequestedPublicationDataSet(DataSetStatus dataSetStatus)
        {
            var publicationId1 = Guid.NewGuid();
            var publicationId2 = Guid.NewGuid();

            var publication1DataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(dataSetStatus)
                .WithPublicationId(publicationId1)
                .Generate();

            var publication2DataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(dataSetStatus)
                .WithPublicationId(publicationId2)
                .Generate();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSets.AddRange(publication1DataSet, publication2DataSet));

            var publication1DataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSet(publication1DataSet)
                .FinishWith(dsv => publication1DataSet.LatestVersion = dsv)
                .Generate();

            var publication2DataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSet(publication2DataSet)
                .FinishWith(dsv => publication2DataSet.LatestVersion = dsv)
                .Generate();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.AddRange(publication1DataSetVersion, publication2DataSetVersion);
                context.DataSets.UpdateRange(publication1DataSet, publication2DataSet);
            });

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client,
                publicationId: publicationId1,
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<DataSetPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(1, content.Paging.TotalResults);
            var result = Assert.Single(content.Results);
            Assert.Equal(publication1DataSet.Id, result.Id);
        }

        [Theory]
        [InlineData(DataSetStatus.Staged)]
        [InlineData(DataSetStatus.Unpublished)]
        public async Task DataSetUnavailable_Returns200_EmptyList(DataSetStatus dataSetStatus)
        {
            var publicationId = Guid.NewGuid();

            var dataSet = DataFixture
                .DefaultDataSet()
                .WithStatus(dataSetStatus)
                .WithPublicationId(publicationId)
                .Generate();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var dataSetVersion = DataFixture
                .DefaultDataSetVersion(
                    filters: 1,
                    indicators: 1,
                    locations: 1,
                    timePeriods: 3)
                .WithStatusPublished()
                .WithDataSet(dataSet)
                .FinishWith(dsv => dataSet.LatestVersion = dsv)
                .Generate();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            });

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client,
                publicationId: publicationId,
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<DataSetPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(0, content.Paging.TotalResults);
            Assert.Empty(content.Results);
        }

        [Fact]
        public async Task NoDataSets_Returns200_EmptyList()
        {
            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client,
                publicationId: Guid.NewGuid(),
                page: 1,
                pageSize: 1);

            var content = response.AssertOk<DataSetPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(0, content.Paging.TotalResults);
            Assert.Empty(content.Results);
        }

        [Fact]
        public async Task PageTooBig_Returns200_EmptyList()
        {
            var page = 2;
            var pageSize = 2;
            var numberOfPublishedDataSets = 2;

            var publicationId = Guid.NewGuid();

            var dataSets = DataFixture
                .DefaultDataSet()
                .WithStatusPublished()
                .WithPublicationId(publicationId)
                .GenerateList(numberOfPublishedDataSets);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.AddRange(dataSets));

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(
                client: client,
                publicationId: publicationId,
                page: page,
                pageSize: pageSize);

            var content = response.AssertOk<DataSetPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(page, content.Paging.Page);
            Assert.Equal(pageSize, content.Paging.PageSize);
            Assert.Equal(numberOfPublishedDataSets, content.Paging.TotalResults);
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
