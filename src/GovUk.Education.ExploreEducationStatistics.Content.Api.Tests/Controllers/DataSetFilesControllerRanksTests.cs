using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures.Optimised;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.WebUtilities;
using MockQueryable.Moq;
using Moq;
using Xunit;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

// ReSharper disable once ClassNeverInstantiated.Global
public class DataSetFilesControllerRanksTestsFixture()
    : OptimisedContentApiCollectionFixture(capabilities: [ContentApiIntegrationTestCapability.Azurite])
{
    public Mock<ContentDbContext> ContentDbContextMock = null!;

    protected override void ConfigureServicesAndConfiguration(
        OptimisedServiceAndConfigModifications serviceModifications
    )
    {
        base.ConfigureServicesAndConfiguration(serviceModifications);

        ContentDbContextMock = new Mock<ContentDbContext>();
        serviceModifications.ReplaceService(ContentDbContextMock.Object);
    }
}

[CollectionDefinition(nameof(DataSetFilesControllerRanksTestsFixture))]
public class DataSetFilesControllerRanksTestsCollection : ICollectionFixture<DataSetFilesControllerRanksTestsFixture>;

[Collection(nameof(DataSetFilesControllerRanksTestsFixture))]
public abstract class DataSetFilesControllerRanksTests(DataSetFilesControllerRanksTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture DataFixture = new();

    public class ListDataSetFilesTests(DataSetFilesControllerRanksTestsFixture fixture)
        : DataSetFilesControllerRanksTests(fixture)
    {
        public class FilterTests(DataSetFilesControllerRanksTestsFixture fixture) : ListDataSetFilesTests(fixture)
        {
            [Fact]
            public async Task FilterBySearchTerm_Success()
            {
                Publication publication = DataFixture
                    .DefaultPublication()
                    .WithReleases(_ => [DataFixture.DefaultRelease(publishedVersions: 1)])
                    .WithTheme(DataFixture.DefaultTheme());

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.Releases[0].Versions[0]);

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(release1Version1Files[0].Id, 1),
                    new(release1Version1Files[1].Id, 2),
                };

                SetupContentDbContextExpectations(
                    publication.Releases.SelectMany(r => r.Versions),
                    release1Version1Files,
                    freeTextRanks
                );

                var query = new DataSetFileListRequest(SearchTerm: "aaa");
                var response = await ListDataSetFiles(query);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile> { release1Version1Files[1], release1Version1Files[0] };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterBySearchTermWhereTermIsNotFound_ReturnsEmpty()
            {
                Publication publication = DataFixture
                    .DefaultPublication()
                    .WithReleases(_ => [DataFixture.DefaultRelease(publishedVersions: 1)])
                    .WithTheme(DataFixture.DefaultTheme());

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.Releases[0].Versions[0]);

                SetupContentDbContextExpectations(
                    publication.Releases.SelectMany(r => r.Versions),
                    release1Version1Files
                );

                var query = new DataSetFileListRequest(SearchTerm: "aaa");
                var response = await ListDataSetFiles(query);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }
        }

        public class SortByTests(DataSetFilesControllerRanksTestsFixture fixture) : ListDataSetFilesTests(fixture)
        {
            [Fact]
            public async Task SortByRelevanceAscending_SortsByRelevanceInAscendingOrder()
            {
                Publication publication = DataFixture
                    .DefaultPublication()
                    .WithReleases(_ => [DataFixture.DefaultRelease(publishedVersions: 1)])
                    .WithTheme(DataFixture.DefaultTheme());

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(
                    publication.Releases[0].Versions[0],
                    numberOfDataSets: 3
                );

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(release1Version1Files[0].Id, 2),
                    new(release1Version1Files[1].Id, 3),
                    new(release1Version1Files[2].Id, 1),
                };

                SetupContentDbContextExpectations(
                    publication.Releases.SelectMany(r => r.Versions),
                    release1Version1Files,
                    freeTextRanks
                );

                var query = new DataSetFileListRequest
                {
                    SearchTerm = "aaa",
                    Sort = DataSetsListRequestSortBy.Relevance,
                    SortDirection = SortDirection.Asc,
                };
                var response = await ListDataSetFiles(query);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    release1Version1Files[2], // Has rank 1
                    release1Version1Files[0], // Has rank 2
                    release1Version1Files[1], //  Has rank 3
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Theory]
            [InlineData(SortDirection.Desc)]
            [InlineData(null)]
            public async Task SortByRelevance_SortsByRelevanceInDescendingOrderAndIsDescendingByDefault(
                SortDirection? sortDirection
            )
            {
                Publication publication = DataFixture
                    .DefaultPublication()
                    .WithReleases(_ => [DataFixture.DefaultRelease(publishedVersions: 1)])
                    .WithTheme(DataFixture.DefaultTheme());

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(
                    publication.Releases[0].Versions[0],
                    numberOfDataSets: 3
                );

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(release1Version1Files[0].Id, 2),
                    new(release1Version1Files[1].Id, 3),
                    new(release1Version1Files[2].Id, 1),
                };

                SetupContentDbContextExpectations(
                    publication.Releases.SelectMany(r => r.Versions),
                    release1Version1Files,
                    freeTextRanks
                );

                var query = new DataSetFileListRequest
                {
                    SearchTerm = "aaa",
                    Sort = DataSetsListRequestSortBy.Relevance,
                    SortDirection = sortDirection,
                };
                var response = await ListDataSetFiles(query);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    release1Version1Files[1], // Has rank 3
                    release1Version1Files[0], // Has rank 2
                    release1Version1Files[2], // Has rank 1
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }
        }

        public class SupersededPublicationTests(DataSetFilesControllerRanksTestsFixture fixture)
            : ListDataSetFilesTests(fixture)
        {
            [Theory]
            [InlineData("aaa")]
            [InlineData("aaaa")]
            public async Task SearchTermAboveMinimumLength_Success(string searchTerm)
            {
                SetupContentDbContextExpectations();

                var query = new DataSetFileListRequest(SearchTerm: searchTerm);
                var response = await ListDataSetFiles(query);

                response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();
            }

            [Fact]
            public async Task SortByRelevanceWithSearchTerm_Success()
            {
                SetupContentDbContextExpectations();

                var query = new DataSetFileListRequest(SearchTerm: "aaa", Sort: DataSetsListRequestSortBy.Relevance);
                var response = await ListDataSetFiles(query);

                response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();
            }
        }

        private async Task<HttpResponseMessage> ListDataSetFiles(DataSetFileListRequest request)
        {
            var queryParams = new Dictionary<string, string?>
            {
                { "themeId", request.ThemeId?.ToString() },
                { "publicationId", request.PublicationId?.ToString() },
                { "releaseId", request.ReleaseId?.ToString() },
                { "geographicLevel", request.GeographicLevel },
                { "latestOnly", request.LatestOnly?.ToString() },
                { "searchTerm", request.SearchTerm },
                { "sort", request.Sort?.ToString() },
                { "sortDirection", request.SortDirection?.ToString() },
                { "page", request.Page.ToString() },
                { "pageSize", request.PageSize.ToString() },
                { "dataSetType", request.DataSetType?.ToString() },
            };

            var uri = QueryHelpers.AddQueryString("/api/data-set-files", queryParams);

            return await fixture.CreateClient().GetAsync(uri);
        }

        private static void AssertResultsForExpectedReleaseFiles(
            List<ReleaseFile> releaseFiles,
            List<DataSetFileSummaryViewModel> viewModels
        )
        {
            Assert.Equal(releaseFiles.Count, viewModels.Count);
            Assert.All(
                releaseFiles.Zip(viewModels),
                tuple =>
                {
                    var (releaseFile, viewModel) = tuple;

                    var releaseVersion = releaseFile.ReleaseVersion;
                    var release = releaseVersion.Release;
                    var publication = release.Publication;
                    var theme = publication.Theme;

                    Assert.Multiple(
                        () => Assert.Equal(releaseFile.FileId, viewModel.FileId),
                        () => Assert.Equal(releaseFile.File.Filename, viewModel.Filename),
                        () => Assert.Equal(releaseFile.File.DisplaySize(), viewModel.FileSize),
                        () => Assert.Equal("csv", viewModel.FileExtension),
                        () => Assert.Equal(releaseFile.Name, viewModel.Title),
                        () => Assert.Equal(releaseFile.Summary, viewModel.Content),
                        () => Assert.Equal(releaseVersion.Id, viewModel.Release.Id),
                        () => Assert.Equal(release.Title, viewModel.Release.Title),
                        () => Assert.Equal(release.Slug, viewModel.Release.Slug),
                        () => Assert.Equal(publication.Id, viewModel.Publication.Id),
                        () => Assert.Equal(publication.Title, viewModel.Publication.Title),
                        () => Assert.Equal(publication.Slug, viewModel.Publication.Slug),
                        () => Assert.Equal(theme.Id, viewModel.Theme.Id),
                        () => Assert.Equal(theme.Title, viewModel.Theme.Title),
                        () =>
                            Assert.Equal(
                                releaseVersion.Id == publication.LatestPublishedReleaseVersionId,
                                viewModel.LatestData
                            ),
                        () =>
                            Assert.Equal(
                                publication.SupersededBy != null
                                    && publication.SupersededBy.LatestPublishedReleaseVersionId != null,
                                viewModel.IsSuperseded
                            ),
                        () => Assert.Equal(releaseFile.ReleaseVersion.Published!.Value, viewModel.Published),
                        () => Assert.Equal(releaseFile.PublicApiDataSetId, viewModel.Api?.Id),
                        () => Assert.Equal(releaseFile.PublicApiDataSetVersionString, viewModel.Api?.Version)
                    );
                }
            );
        }

        private List<ReleaseFile> GenerateDataSetFilesForReleaseVersion(
            ReleaseVersion releaseVersion,
            int numberOfDataSets = 2
        )
        {
            return DataFixture
                .DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFiles(
                    DataFixture
                        .DefaultFile(FileType.Data)
                        .WithDataSetFileMeta(DataFixture.DefaultDataSetFileMeta())
                        .WithDataSetFileVersionGeographicLevels([GeographicLevel.Country])
                        .GenerateList(numberOfDataSets)
                )
                .GenerateList();
        }

        private void SetupContentDbContextExpectations(
            IEnumerable<ReleaseVersion>? releaseVersions = null,
            IEnumerable<ReleaseFile>? releaseFiles = null,
            IEnumerable<FreeTextRank>? freeTextRanks = null
        )
        {
            var releaseFilesArray = releaseFiles?.ToArray() ?? [];

            var contentDbContext = fixture.ContentDbContextMock;
            contentDbContext
                .Setup(context => context.ReleaseVersions)
                .Returns((releaseVersions ?? []).ToArray().BuildMockDbSet().Object);
            contentDbContext.Setup(context => context.ReleaseFiles).Returns(releaseFilesArray.BuildMockDbSet().Object);
            contentDbContext
                .Setup(context => context.Files)
                .Returns(releaseFilesArray.Select(rf => rf.File).ToArray().BuildMockDbSet().Object);
            contentDbContext
                .Setup(context => context.ReleaseFilesFreeTextTable(It.IsAny<string>()))
                .Returns((freeTextRanks ?? []).ToArray().BuildMockDbSet().Object);
        }
    }
}
