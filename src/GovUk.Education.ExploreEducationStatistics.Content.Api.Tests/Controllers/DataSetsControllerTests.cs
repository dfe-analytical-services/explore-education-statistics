#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class DataSetsControllerTests : IntegrationTest<TestStartup>
{
    private readonly DataFixture _fixture = new();

    public DataSetsControllerTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
    {
    }

    public class ListDataSetsTests : DataSetsControllerTests
    {
        public ListDataSetsTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task Success()
        {
            var query = new DataSetsListRequest();

            var cacheKey = new ListDataSetsCacheKey(query);

            MemoryCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(PaginatedListViewModel<DataSetListViewModel>)))
                .Returns((object?) null);

            MemoryCacheService
                .Setup(s => s.SetItem<object>(
                    cacheKey,
                    It.IsAny<PaginatedListViewModel<DataSetListViewModel>>(),
                    It.IsAny<MemoryCacheConfiguration>(),
                    null));

            var (releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

            var client = BuildApp()
                .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                .CreateClient();

            var response = await ListDataSets(client, query);

            MockUtils.VerifyAllMocks(MemoryCacheService);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

            var expectedReleaseFiles = new List<ReleaseFile>
            {
                releaseFilesRelease1[0],
                releaseFilesRelease1[1],
                releaseFilesRelease2[0],
                releaseFilesRelease2[1]
            };

            AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
            AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
        }

        [Fact]
        public async Task NoPublishedDataSets_ReturnsEmpty()
        {
            var query = new DataSetsListRequest();

            var cacheKey = new ListDataSetsCacheKey(query);

            MemoryCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(PaginatedListViewModel<DataSetListViewModel>)))
                .Returns((object?) null);

            MemoryCacheService
                .Setup(s => s.SetItem<object>(
                    cacheKey,
                    It.IsAny<PaginatedListViewModel<DataSetListViewModel>>(),
                    It.IsAny<MemoryCacheConfiguration>(),
                    null));

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(client, query);

            MockUtils.VerifyAllMocks(MemoryCacheService);

            var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

            AssertPaginatedViewModel(pagedResult, expectedTotalResults: 0);
            Assert.Empty(pagedResult.Results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task PageTooSmall_ReturnsBadRequest(int page)
        {
            var query = new DataSetsListRequest(
                Page: page
            );

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(client, query);

            response.AssertBadRequest(expectedErrorKey: "Page",
                expectedErrorMessage: "'Page' must be greater than or equal to '1'.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task PageSizeTooSmall_ReturnsBadRequest(int pageSize)
        {
            var query = new DataSetsListRequest(
                PageSize: pageSize
            );

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(client, query);

            response.AssertBadRequest(expectedErrorKey: "PageSize",
                expectedErrorMessage: "'Page Size' must be greater than or equal to '1'.");
        }

        [Theory]
        [InlineData(41)]
        public async Task PageSizeTooBig_ReturnsBadRequest(int pageSize)
        {
            var query = new DataSetsListRequest(
                PageSize: pageSize
            );

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(client, query);

            response.AssertBadRequest(expectedErrorKey: "PageSize",
                expectedErrorMessage: "'Page Size' must be less than or equal to '40'.");
        }

        [Theory]
        [InlineData("a")]
        [InlineData("aa")]
        public async Task SearchTermTooSmall_ReturnsBadRequest(string searchTerm)
        {
            var query = new DataSetsListRequest(
                SearchTerm: searchTerm
            );

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(client, query);

            MockUtils.VerifyAllMocks(MemoryCacheService);

            response.AssertBadRequest(expectedErrorKey: "SearchTerm",
                expectedErrorMessage:
                $"The length of 'Search Term' must be at least 3 characters. You entered {searchTerm.Length} characters.");
        }

        [Fact]
        public async Task OrderByNaturalWithoutReleaseId_ReturnsBadRequest()
        {
            var query = new DataSetsListRequest(
                OrderBy: DataSetsListRequestOrderBy.Natural
            );

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(client, query);

            MockUtils.VerifyAllMocks(MemoryCacheService);

            response.AssertBadRequest(expectedErrorKey: "ReleaseId",
                expectedErrorMessage: "'Release Id' must not be empty.");
        }

        [Fact]
        public async Task OrderByRelevanceWithoutSearchTerm_ReturnsBadRequest()
        {
            var query = new DataSetsListRequest(
                OrderBy: DataSetsListRequestOrderBy.Relevance
            );

            var client = BuildApp().CreateClient();

            var response = await ListDataSets(client, query);

            MockUtils.VerifyAllMocks(MemoryCacheService);

            response.AssertBadRequest(expectedErrorKey: "SearchTerm",
                expectedErrorMessage: "'Search Term' must not be empty.");
        }

        private static async Task<HttpResponseMessage> ListDataSets(HttpClient client,
            DataSetsListRequest request)
        {
            var queryParams = new Dictionary<string, string?>
            {
                { "themeId", request.ThemeId?.ToString() },
                { "publicationId", request.PublicationId?.ToString() },
                { "releaseId", request.ReleaseId?.ToString() },
                { "latest", request.Latest?.ToString() },
                { "searchTerm", request.SearchTerm },
                { "orderBy", request.OrderBy?.ToString() },
                { "sort", request.Sort?.ToString() },
                { "page", request.Page.ToString() },
                { "pageSize", request.PageSize.ToString() }
            };

            var uri = QueryHelpers.AddQueryString("/api/data-sets", queryParams);

            return await client.GetAsync(uri);
        }

        private static void AssertPaginatedViewModel(PaginatedListViewModel<DataSetListViewModel> pagedResult,
            int expectedTotalResults,
            int expectedPage = 1,
            int expectedPageSize = 10)
        {
            Assert.Multiple(
                () => Assert.Equal(expectedTotalResults, pagedResult.Paging.TotalResults),
                () => Assert.Equal(expectedPage, pagedResult.Paging.Page),
                () => Assert.Equal(expectedPageSize, pagedResult.Paging.PageSize)
            );
        }

        private static void AssertResultsForExpectedReleaseFiles(
            List<ReleaseFile> releaseFiles,
            List<DataSetListViewModel> viewModels)
        {
            Assert.Equal(releaseFiles.Count, viewModels.Count);
            Assert.All(releaseFiles.Zip(viewModels), tuple =>
            {
                var (releaseFile, viewModel) = tuple;

                var release = releaseFile.Release;
                var publication = release.Publication;
                var theme = publication.Topic.Theme;

                Assert.Multiple(
                    () => Assert.Equal(releaseFile.FileId, viewModel.FileId),
                    () => Assert.Equal(releaseFile.File.Filename, viewModel.Filename),
                    () => Assert.Equal(releaseFile.File.DisplaySize(), viewModel.FileSize),
                    () => Assert.Equal("csv", viewModel.FileExtension),
                    () => Assert.Equal(releaseFile.Name, viewModel.Title),
                    () => Assert.Equal(releaseFile.Summary, viewModel.Content),
                    () => Assert.Equal(release.Id, viewModel.Release.Id),
                    () => Assert.Equal(release.Title, viewModel.Release.Title),
                    () => Assert.Equal(publication.Id, viewModel.Publication.Id),
                    () => Assert.Equal(publication.Title, viewModel.Publication.Title),
                    () => Assert.Equal(theme.Id, viewModel.Theme.Id),
                    () => Assert.Equal(theme.Title, viewModel.Theme.Title),
                    () => Assert.True(viewModel.LatestData),
                    () => Assert.Equal(releaseFile.Release.Published!.Value, viewModel.Published)
                );
            });
        }

        private (
            List<ReleaseFile> releaseFilesRelease1,
            List<ReleaseFile> releaseFilesRelease2,
            List<ReleaseFile> allReleaseFiles
            ) GenerateTestData()
        {
            // Generate releases which are all associated with unique publications / topics / themes
            var (release1, release2, unpublishedRelease) = _fixture
                .DefaultRelease()
                .WithPublications(
                    _fixture.DefaultPublication()
                        .WithTopics(
                            _fixture.DefaultTopic()
                                .WithThemes(
                                    _fixture.DefaultTheme()
                                        .GenerateList(3))
                                .GenerateList())
                        .GenerateList())
                .ForIndex(0, s => s.SetPublished(DateTime.UtcNow.AddDays(-1)))
                .ForIndex(1, s => s.SetPublished(DateTime.UtcNow.AddDays(-2)))
                .GenerateList()
                .ToTuple3();

            // Set each of the published releases as the latest published release for their publication
            release1.Publication.LatestPublishedReleaseId = release1.Id;
            release2.Publication.LatestPublishedReleaseId = release2.Id;

            // Generate two data set files per release, initialised with a reversed order so that we can easily verify if the
            // sorting routine correctly handles the natural order set by analysts.
            var releaseFilesRelease1 = _fixture.DefaultReleaseFile()
                .WithRelease(release1)
                .ForIndex(0, s => s.SetOrder(1))
                .ForIndex(1, s => s.SetOrder(0))
                .WithFiles(_fixture.DefaultFile()
                    .GenerateList(2))
                .GenerateList();

            var releaseFilesRelease2 = _fixture.DefaultReleaseFile()
                .WithRelease(release2)
                .ForIndex(0, s => s.SetOrder(1))
                .ForIndex(1, s => s.SetOrder(0))
                .WithFiles(_fixture.DefaultFile()
                    .GenerateList(2))
                .GenerateList();

            var releaseFilesUnpublished = _fixture.DefaultReleaseFile()
                .WithRelease(unpublishedRelease)
                .ForIndex(0, s => s.SetOrder(1))
                .ForIndex(1, s => s.SetOrder(0))
                .WithFiles(_fixture.DefaultFile()
                    .GenerateList(2))
                .GenerateList();

            var allReleaseFiles = releaseFilesRelease1
                .Concat(releaseFilesRelease2)
                .Concat(releaseFilesUnpublished)
                .ToList();

            return (releaseFilesRelease1,
                releaseFilesRelease2,
                allReleaseFiles);
        }
    }

    private WebApplicationFactory<TestStartup> BuildApp()
    {
        return TestApp
            .ResetDbContexts()
            .ConfigureServices(services =>
            {
                services.AddTransient<IDataSetService, DataSetService>(
                    s =>
                    {
                        var contentDbContext = s.GetRequiredService<ContentDbContext>();
                        return new DataSetService(contentDbContext);
                    }
                );
            });
    }
}
