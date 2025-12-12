using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Search;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using PublicationSummaryViewModel = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.PublicationSummaryViewModel;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global
public class PublicationsControllerTestsFixture()
    : OptimisedPublicApiCollectionFixture(
        capabilities: [PublicApiIntegrationTestCapability.UserAuth, PublicApiIntegrationTestCapability.Postgres]
    );

[CollectionDefinition(nameof(PublicationsControllerTestsFixture))]
public class PublicationControllerTestsCollection : ICollectionFixture<PublicationsControllerTestsFixture>;

[Collection(nameof(PublicationsControllerTestsFixture))]
public abstract class PublicationsControllerTests(PublicationsControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private const string BaseUrl = "v1/publications";

    private static readonly DataFixture DataFixture = new();

    public abstract class ListPublicationsTests(PublicationsControllerTestsFixture fixture)
        : PublicationsControllerTests(fixture),
            IAsyncLifetime
    {
        public new async Task InitializeAsync()
        {
            await base.InitializeAsync();

            // As the "ListPublications" method does not allow us to call it with parameters that let us control
            // what data it acts upon, we have to clear down any published data sets so we're acting on a clean
            // slate between test methods.
            await fixture.GetPublicDataDbContext().ClearTestData();
        }

        [Fact]
        public async Task PublishedDataSets_Returns200_FiltersPublicationsWithoutPublishedDataSets()
        {
            await fixture.GetPublicDataDbContext().ClearTestData();

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
                .WithStatus(DataSetStatus.Withdrawn)
                .GenerateList(numberOfUnpublishedDataSets);

            var allDataSets = publishedDataSets.Concat(unpublishedDataSets).ToArray();

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.AddRange(allDataSets));

            var publishedDataSetPublicationIds = publishedDataSets.Select(p => p.PublicationId).ToList();

            var expectedPublicationIds = publishedDataSetPublicationIds.Take(2).ToList();

            var publicationSearchResults = expectedPublicationIds
                .Select(id =>
                    DataFixture
                        .Generator<PublicationSearchResult>()
                        .ForInstance(s =>
                            s.Set(p => p.PublicationId, id)
                                .SetDefault(p => p.PublicationSlug)
                                .Set(p => p.Published, p => p.Date.Past())
                                .SetDefault(p => p.Summary)
                                .SetDefault(p => p.Title)
                        )
                        .Generate()
                )
                .ToList();

            var searchServiceMock = fixture.GetSearchServiceMock();

            searchServiceMock
                .Setup(c =>
                    c.SearchPublications(
                        page,
                        pageSize,
                        It.Is<IEnumerable<Guid>>(ids =>
                            ids.Order().SequenceEqual(publishedDataSetPublicationIds.Order())
                        ),
                        null,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new Common.ViewModels.PaginatedListViewModel<PublicationSearchResult>(
                        results: publicationSearchResults,
                        totalResults: numberOfPublishedDataSets,
                        page: page,
                        pageSize: pageSize
                    )
                );

            var response = await ListPublications(page: page, pageSize: pageSize);

            MockUtils.VerifyAllMocks(searchServiceMock);

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

            DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished().WithPublicationId(publicationId);

            await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

            PublicationSearchResult publicationSearchResult = DataFixture
                .Generator<PublicationSearchResult>()
                .ForInstance(s =>
                    s.Set(p => p.PublicationId, publicationId)
                        .SetDefault(p => p.PublicationSlug)
                        .Set(p => p.Published, p => p.Date.Past())
                        .SetDefault(p => p.Summary)
                        .SetDefault(p => p.Title)
                );

            var searchServiceMock = fixture.GetSearchServiceMock();

            searchServiceMock
                .Setup(c =>
                    c.SearchPublications(1, 1, new List<Guid> { publicationId }, null, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(
                    new Common.ViewModels.PaginatedListViewModel<PublicationSearchResult>(
                        results: [publicationSearchResult],
                        totalResults: 1,
                        page: 1,
                        pageSize: 1
                    )
                );

            var response = await ListPublications(page: 1, pageSize: 1);

            MockUtils.VerifyAllMocks(searchServiceMock);

            var content = response.AssertOk<PublicationPaginatedListViewModel>(useSystemJson: true);

            Assert.NotNull(content);
            Assert.Equal(1, content.Paging.Page);
            Assert.Equal(1, content.Paging.PageSize);
            Assert.Equal(1, content.Paging.TotalResults);

            var result = Assert.Single(content.Results);

            Assert.Equal(publicationSearchResult.PublicationId, result.Id);
            Assert.Equal(publicationSearchResult.Title, result.Title);
            Assert.Equal(publicationSearchResult.PublicationSlug, result.Slug);
            Assert.Equal(publicationSearchResult.Summary, result.Summary);
            Assert.Equal(publicationSearchResult.Published, result.LastPublished);
        }

        [Fact]
        public async Task NoPublishedDataSets_Returns200_EmptyList()
        {
            var searchServiceMock = fixture.GetSearchServiceMock();

            searchServiceMock
                .Setup(c => c.SearchPublications(1, 1, Enumerable.Empty<Guid>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new Common.ViewModels.PaginatedListViewModel<PublicationSearchResult>(
                        results: [],
                        totalResults: 0,
                        page: 1,
                        pageSize: 1
                    )
                );

            var response = await ListPublications(page: 1, pageSize: 1);

            MockUtils.VerifyAllMocks(searchServiceMock);

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
            var response = await ListPublications(page: page, pageSize: 1);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasGreaterThanOrEqualError("page", comparisonValue: 1);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(41)]
        public async Task PageSizeOutOfBounds_Returns400(int pageSize)
        {
            var response = await ListPublications(page: 1, pageSize: pageSize);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasInclusiveBetweenError("pageSize", from: 1, to: 40);
        }

        [Theory]
        [InlineData("a")]
        [InlineData("aa")]
        public async Task SearchTermTooSmall_Returns400(string searchTerm)
        {
            var response = await ListPublications(page: null, pageSize: null, search: searchTerm);

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasMinimumLengthError("search", minLength: 3);
        }

        [Fact]
        public async Task AnalyticsRequestCaptured()
        {
            var searchServiceMock = fixture.GetSearchServiceMock();

            searchServiceMock
                .Setup(c => c.SearchPublications(2, 10, Enumerable.Empty<Guid>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new Common.ViewModels.PaginatedListViewModel<PublicationSearchResult>(
                        results: [],
                        totalResults: 0,
                        page: 2,
                        pageSize: 10
                    )
                );

            var analyticsServiceMock = fixture.GetAnalyticsServiceMock();

            analyticsServiceMock
                .Setup(s =>
                    s.CaptureTopLevelCall(
                        TopLevelCallType.GetPublications,
                        new PaginationParameters(2, 10),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            var response = await ListPublications(page: 2, pageSize: 10);

            MockUtils.VerifyAllMocks(searchServiceMock, analyticsServiceMock);

            response.AssertOk<PublicationPaginatedListViewModel>(useSystemJson: true);
        }

        private async Task<HttpResponseMessage> ListPublications(
            int? page = null,
            int? pageSize = null,
            string? search = null
        )
        {
            var query = new Dictionary<string, string?>
            {
                { "page", page?.ToString() },
                { "pageSize", pageSize?.ToString() },
                { "search", search },
            };

            var uri = QueryHelpers.AddQueryString(BaseUrl, query);

            return await fixture.CreateClient().GetAsync(uri);
        }
    }

    public abstract class GetPublicationTests(PublicationsControllerTestsFixture fixture)
        : PublicationsControllerTests(fixture)
    {
        public class PublishedPublicationsTests(PublicationsControllerTestsFixture fixture)
            : GetPublicationTests(fixture)
        {
            [Fact]
            public async Task PublicationExists_Returns200()
            {
                PublishedPublicationSummaryViewModel publication = DataFixture
                    .Generator<PublishedPublicationSummaryViewModel>()
                    .ForInstance(s =>
                        s.SetDefault(f => f.Id)
                            .SetDefault(f => f.Title)
                            .SetDefault(f => f.Slug)
                            .SetDefault(f => f.Summary)
                            .Set(f => f.Published, f => f.Date.Past())
                    );

                var contentApiClientMock = fixture.GetContentApiClientMock();

                contentApiClientMock
                    .Setup(c => c.GetPublication(publication.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(publication);

                var publishedDataSet = GeneratePublishedDataSet(publication.Id);
                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(publishedDataSet));

                var response = await GetPublication(publication.Id);

                MockUtils.VerifyAllMocks(contentApiClientMock);

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
                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(publishedDataSet));

                var contentApiClientMock = fixture.GetContentApiClientMock();

                contentApiClientMock
                    .Setup(c => c.GetPublication(publicationId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new NotFoundResult());

                var response = await GetPublication(publicationId);

                MockUtils.VerifyAllMocks(contentApiClientMock);

                response.AssertNotFound();
            }

            [Fact]
            public async Task NotDataSets_Returns404()
            {
                var response = await GetPublication(It.IsAny<Guid>());

                response.AssertNotFound();
            }

            [Fact]
            public async Task UnpublishedDataSets_Returns404()
            {
                var publicationId = Guid.NewGuid();

                var unpublishedDataSets = DataFixture
                    .DefaultDataSet()
                    .WithStatus(DataSetStatus.Withdrawn)
                    .WithPublicationId(publicationId)
                    .GenerateList(3);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSets.AddRange(unpublishedDataSets));

                var response = await GetPublication(publicationId);

                response.AssertNotFound();
            }

            [Fact]
            public async Task ContentApiThrowsException_Returns500()
            {
                var publicationId = Guid.NewGuid();

                var publishedDataSet = GeneratePublishedDataSet(publicationId);
                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(publishedDataSet));

                var contentApiClientMock = fixture.GetContentApiClientMock();

                contentApiClientMock
                    .Setup(c => c.GetPublication(publicationId, It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new HttpRequestException("something went wrong"));

                var response = await Assert.ThrowsAsync<HttpRequestException>(() => GetPublication(publicationId));

                MockUtils.VerifyAllMocks(contentApiClientMock);

                // TODO EES-6762 - this suggests that we are at risk of exposing raw exception details on uncaught
                // exceptions, and we should have some global way to handle exceptions. Previously this test was
                // passing because we were using app.UseDeveloperExceptionPage() during integration tests.
                Assert.Equal("something went wrong", response.Message);
            }

            [Fact]
            public async Task AnalyticsRequestCaptured()
            {
                PublishedPublicationSummaryViewModel publication = DataFixture
                    .Generator<PublishedPublicationSummaryViewModel>()
                    .ForInstance(s => s.Set(f => f.Id, Guid.NewGuid))
                    .ForInstance(s => s.Set(f => f.Published, f => f.Date.Past()));

                var contentApiClientMock = fixture.GetContentApiClientMock();

                contentApiClientMock
                    .Setup(c => c.GetPublication(publication.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(publication);

                var publishedDataSet = GeneratePublishedDataSet(publication.Id);
                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(publishedDataSet));

                var analyticsServiceMock = fixture.GetAnalyticsServiceMock();

                analyticsServiceMock
                    .Setup(s =>
                        s.CapturePublicationCall(
                            publication.Id,
                            publication.Title,
                            PublicationCallType.GetSummary,
                            null,
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .Returns(Task.CompletedTask);

                var response = await GetPublication(publication.Id);

                MockUtils.VerifyAllMocks(contentApiClientMock, analyticsServiceMock);

                response.AssertOk<PublicationSummaryViewModel>(useSystemJson: true);
            }
        }

        private async Task<HttpResponseMessage> GetPublication(Guid publicationId)
        {
            var uri = new Uri($"{BaseUrl}/{publicationId}", UriKind.Relative);

            return await fixture.CreateClient().GetAsync(uri);
        }

        private DataSet GeneratePublishedDataSet(Guid publicationId)
        {
            return DataFixture.DefaultDataSet().WithStatus(DataSetStatus.Published).WithPublicationId(publicationId);
        }
    }

    public abstract class ListPublicationDataSetsTests(PublicationsControllerTestsFixture fixture)
        : PublicationsControllerTests(fixture)
    {
        public class ControllerTests(PublicationsControllerTestsFixture fixture) : ListPublicationDataSetsTests(fixture)
        {
            [Theory]
            [InlineData(1, 2, 1)]
            [InlineData(1, 2, 2)]
            [InlineData(1, 2, 9)]
            [InlineData(1, 3, 2)]
            [InlineData(2, 2, 9)]
            public async Task MultipleDataSetsAvailableForRequestedPublication_Returns200(
                int page,
                int pageSize,
                int numberOfAvailableDataSets
            )
            {
                var publicationId = Guid.NewGuid();

                var dataSets = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished()
                    .WithPublicationId(publicationId)
                    .GenerateList(numberOfAvailableDataSets);

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.AddRange(dataSets));

                var dataSetVersions = dataSets
                    .Select(ds =>
                        DataFixture
                            .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                            .WithStatusPublished()
                            .WithDataSet(ds)
                            .FinishWith(dsv => ds.LatestLiveVersion = dsv)
                            .Generate()
                    )
                    .ToList();

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context =>
                    {
                        context.DataSetVersions.AddRange(dataSetVersions);
                        context.DataSets.UpdateRange(dataSets);
                    });

                var expectedDataSetIds = dataSets
                    .OrderByDescending(ds => ds.LatestLiveVersion!.Published)
                    .ThenBy(ds => ds.Title)
                    .ThenBy(ds => ds.Id)
                    .Select(ds => ds.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var response = await ListPublicationDataSets(
                    publicationId: publicationId,
                    page: page,
                    pageSize: pageSize
                );

                var content = response.AssertOk<DataSetPaginatedListViewModel>(useSystemJson: true);

                Assert.NotNull(content);
                Assert.Equal(page, content.Paging.Page);
                Assert.Equal(pageSize, content.Paging.PageSize);
                Assert.Equal(numberOfAvailableDataSets, content.Paging.TotalResults);
                Assert.Equal(expectedDataSetIds.Count, content.Results.Count);

                Assert.All(content.Results, r => Assert.Contains(expectedDataSetIds, id => id == r.Id));
            }

            [Theory]
            [MemberData(
                nameof(DataSetStatusTheoryData.AvailableStatuses),
                MemberType = typeof(DataSetStatusTheoryData)
            )]
            public async Task DataSetIsAvailable_Returns200_CorrectViewModel(DataSetStatus dataSetStatus)
            {
                var publicationId = Guid.NewGuid();

                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(dataSetStatus)
                    .WithPublicationId(publicationId);

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context =>
                    {
                        context.DataSetVersions.Add(dataSetVersion);
                        context.DataSets.Update(dataSet);
                    });

                var response = await ListPublicationDataSets(publicationId: publicationId, page: 1, pageSize: 1);

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
                Assert.NotNull(result.LatestVersion);
                Assert.Equal(dataSetVersion.PublicVersion, result.LatestVersion.Version);
                Assert.Equal(dataSetVersion.Published.TruncateNanoseconds(), result.LatestVersion.Published);
                Assert.Equal(dataSetVersion.TotalResults, result.LatestVersion.TotalResults);
                Assert.Equal(dataSetVersion.Release.DataSetFileId, result.LatestVersion.File.Id);
                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary!.TimePeriodRange.Start.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.Start.Code
                    ),
                    result.LatestVersion.TimePeriods.Start
                );
                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Code
                    ),
                    result.LatestVersion.TimePeriods.End
                );
                Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, result.LatestVersion.GeographicLevels);
                Assert.Equal(dataSetVersion.MetaSummary.Filters, result.LatestVersion.Filters);
                Assert.Equal(dataSetVersion.MetaSummary.Indicators, result.LatestVersion.Indicators);
            }

            [Fact]
            public async Task DataSetAvailableForOtherPublication_Returns200_OnlyRequestedPublicationDataSet()
            {
                var publicationId1 = Guid.NewGuid();
                var publicationId2 = Guid.NewGuid();

                DataSet publication1DataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished()
                    .WithPublicationId(publicationId1);

                DataSet publication2DataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished()
                    .WithPublicationId(publicationId2);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context => context.DataSets.AddRange(publication1DataSet, publication2DataSet));

                DataSetVersion publication1DataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSet(publication1DataSet)
                    .FinishWith(dsv => publication1DataSet.LatestLiveVersion = dsv);

                DataSetVersion publication2DataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSet(publication2DataSet)
                    .FinishWith(dsv => publication2DataSet.LatestLiveVersion = dsv);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context =>
                    {
                        context.DataSetVersions.AddRange(publication1DataSetVersion, publication2DataSetVersion);
                        context.DataSets.UpdateRange(publication1DataSet, publication2DataSet);
                    });

                var response = await ListPublicationDataSets(publicationId: publicationId1, page: 1, pageSize: 1);

                var content = response.AssertOk<DataSetPaginatedListViewModel>(useSystemJson: true);

                Assert.NotNull(content);
                Assert.Equal(1, content.Paging.Page);
                Assert.Equal(1, content.Paging.PageSize);
                Assert.Equal(1, content.Paging.TotalResults);
                var result = Assert.Single(content.Results);
                Assert.Equal(publication1DataSet.Id, result.Id);
            }

            [Theory]
            [MemberData(
                nameof(DataSetStatusTheoryData.UnavailableStatuses),
                MemberType = typeof(DataSetStatusTheoryData)
            )]
            public async Task DataSetUnavailable_Returns200_EmptyList(DataSetStatus dataSetStatus)
            {
                var publicationId = Guid.NewGuid();

                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(dataSetStatus)
                    .WithPublicationId(publicationId);

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context =>
                    {
                        context.DataSetVersions.Add(dataSetVersion);
                        context.DataSets.Update(dataSet);
                    });

                var response = await ListPublicationDataSets(publicationId: publicationId, page: 1, pageSize: 1);

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
                var response = await ListPublicationDataSets(publicationId: Guid.NewGuid(), page: 1, pageSize: 1);

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

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.AddRange(dataSets));

                var response = await ListPublicationDataSets(
                    publicationId: publicationId,
                    page: page,
                    pageSize: pageSize
                );

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
                var response = await ListPublicationDataSets(publicationId: Guid.NewGuid(), page: page, pageSize: 1);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasGreaterThanOrEqualError("page", comparisonValue: 1);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(21)]
            public async Task PageSizeOutOfBounds_Returns400(int pageSize)
            {
                var response = await ListPublicationDataSets(
                    publicationId: Guid.NewGuid(),
                    page: 1,
                    pageSize: pageSize
                );

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasInclusiveBetweenError("pageSize", from: 1, to: 20);
            }

            [Fact]
            public async Task InvalidPublicationId_Returns404()
            {
                var query = new Dictionary<string, string?> { { "page", "1" }, { "pageSize", "1" } };

                var uri = QueryHelpers.AddQueryString($"{BaseUrl}/not_a_valid_guid/data-sets", query);

                var response = await fixture.CreateClient().GetAsync(uri);

                response.AssertNotFound();
            }

            [Fact]
            public async Task AnalyticsRequestCaptured()
            {
                var publication = new PublishedPublicationSummaryViewModel
                {
                    Id = Guid.NewGuid(),
                    Title = "Publication 1",
                };

                var analyticsServiceMock = fixture.GetAnalyticsServiceMock();

                analyticsServiceMock
                    .Setup(s =>
                        s.CapturePublicationCall(
                            publication.Id,
                            PublicationCallType.GetDataSets,
                            new PaginationParameters(1, 10),
                            It.IsAny<CancellationToken>()
                        )
                    )
                    .Returns(Task.CompletedTask);

                DataSet dataSet = DataFixture.DefaultDataSet().WithStatusPublished().WithPublicationId(publication.Id);

                await fixture.GetPublicDataDbContext().AddTestData(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await fixture
                    .GetPublicDataDbContext()
                    .AddTestData(context =>
                    {
                        context.DataSetVersions.Add(dataSetVersion);
                        context.DataSets.Update(dataSet);
                    });

                var response = await ListPublicationDataSets(publicationId: publication.Id, page: 1, pageSize: 10);

                MockUtils.VerifyAllMocks(analyticsServiceMock);

                response.AssertOk<DataSetPaginatedListViewModel>(useSystemJson: true);
            }
        }

        private async Task<HttpResponseMessage> ListPublicationDataSets(
            Guid publicationId,
            int? page = null,
            int? pageSize = null
        )
        {
            var query = new Dictionary<string, string?>
            {
                { "page", page?.ToString() },
                { "pageSize", pageSize?.ToString() },
            };

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{publicationId}/data-sets", query);

            return await fixture.CreateClient().GetAsync(uri);
        }
    }
}
