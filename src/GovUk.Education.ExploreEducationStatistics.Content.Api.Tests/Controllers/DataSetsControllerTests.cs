#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
using MockQueryable.Moq;
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

        public class FilterTests : ListDataSetsTests
        {
            public FilterTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Fact]
            public async Task FilterByReleaseId_Success()
            {
                var (release1, _, _, releaseFilesRelease1, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    ReleaseId = release1.Id
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease1[0],
                    releaseFilesRelease1[1]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterByPublicationId_Success()
            {
                var (release1, _, _, releaseFilesRelease1, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    PublicationId = release1.PublicationId
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease1[0],
                    releaseFilesRelease1[1]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterByThemeId_Success()
            {
                var (release1, _, _, releaseFilesRelease1, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    ThemeId = release1.Publication.Topic.ThemeId
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease1[0],
                    releaseFilesRelease1[1]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterByReleaseIdWhereReleaseIsNotPublished_ReturnsEmpty()
            {
                var (_, _, unpublishedRelease, _, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    ReleaseId = unpublishedRelease.Id
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: 0);
                Assert.Empty(pagedResult.Results);
            }

            [Fact]
            public async Task FilterByPublicationIdWherePublicationIsNotPublished_ReturnsEmpty()
            {
                var (_, _, unpublishedRelease, _, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    PublicationId = unpublishedRelease.PublicationId
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: 0);
                Assert.Empty(pagedResult.Results);
            }

            [Fact]
            public async Task FilterByThemeIdWhereThemeIsNotPublished_ReturnsEmpty()
            {
                var (_, _, unpublishedRelease, _, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    ThemeId = unpublishedRelease.Publication.Topic.ThemeId
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: 0);
                Assert.Empty(pagedResult.Results);
            }

            [Fact]
            public async Task FilterBySearchTerm_Success()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(releaseFilesRelease2[1].Id, 1),
                    new(releaseFilesRelease1[1].Id, 2),
                    new(releaseFilesRelease1[0].Id, 3),
                    new(releaseFilesRelease2[0].Id, 4)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = new Mock<ContentDbContext>();
                contentDbContext.Setup(context => context.ReleaseFiles)
                    .Returns(allReleaseFiles.AsQueryable().BuildMockDbSet().Object);
                contentDbContext.Setup(context => context.ReleaseFilesFreeTextTable("aaa"))
                    .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    SearchTerm = "aaa"
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease2[0],
                    releaseFilesRelease1[0],
                    releaseFilesRelease1[1],
                    releaseFilesRelease2[1]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterBySearchTermWhereTermIsNotFound_ReturnsEmpty()
            {
                var (_, _, _, _, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = new Mock<ContentDbContext>();
                contentDbContext.Setup(context => context.ReleaseFiles)
                    .Returns(allReleaseFiles.AsQueryable().BuildMockDbSet().Object);
                contentDbContext.Setup(context => context.ReleaseFilesFreeTextTable("aaa"))
                    .Returns(new List<FreeTextRank>().AsQueryable().BuildMockDbSet().Object);

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    SearchTerm = "aaa"
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: 0);
                Assert.Empty(pagedResult.Results);
            }

            [Fact]
            public async Task NoFilter_ReturnsAllResultsOrderedByTitleAscending()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest();
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
        }

        public class OrderByTests : ListDataSetsTests
        {
            public OrderByTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Fact]
            public async Task OrderByTitleAscending_Success()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    OrderBy = DataSetsListRequestOrderBy.Title,
                    Sort = SortOrder.Asc
                };
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
            public async Task OrderByTitleDescending_Success()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    OrderBy = DataSetsListRequestOrderBy.Title,
                    Sort = SortOrder.Desc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease2[1],
                    releaseFilesRelease2[0],
                    releaseFilesRelease1[1],
                    releaseFilesRelease1[0]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByTitleWithoutSortOrder_DefaultsToAscending()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    OrderBy = DataSetsListRequestOrderBy.Title,
                    Sort = null
                };
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
            public async Task OrderByNaturalAscending_Success()
            {
                var (release1, _, _, releaseFilesRelease1, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    ReleaseId = release1.Id,
                    OrderBy = DataSetsListRequestOrderBy.Natural,
                    Sort = SortOrder.Asc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease1[1], // Has natural order 0
                    releaseFilesRelease1[0] // Has natural order 1
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByNaturalDescending_Success()
            {
                var (release1, _, _, releaseFilesRelease1, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    ReleaseId = release1.Id,
                    OrderBy = DataSetsListRequestOrderBy.Natural,
                    Sort = SortOrder.Desc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease1[0], // Has natural order 1
                    releaseFilesRelease1[1] // Has natural order 0
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByNaturalWithoutSortOrder_DefaultsToAscending()
            {
                var (release1, _, _, releaseFilesRelease1, _, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    ReleaseId = release1.Id,
                    OrderBy = DataSetsListRequestOrderBy.Natural,
                    Sort = null
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease1[1], // Has natural order 0
                    releaseFilesRelease1[0] // Has natural order 1
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByPublishedAscending_Success()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    OrderBy = DataSetsListRequestOrderBy.Published,
                    Sort = SortOrder.Asc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Expect data set files belonging to the oldest published release to be returned first
                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease2[0],
                    releaseFilesRelease2[1],
                    releaseFilesRelease1[0],
                    releaseFilesRelease1[1]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByPublishedDescending_Success()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    OrderBy = DataSetsListRequestOrderBy.Published,
                    Sort = SortOrder.Desc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Expect data set files belonging to the newest published release to be returned first
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
            public async Task OrderByPublishedWithoutSortOrder_DefaultsToDescending()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    OrderBy = DataSetsListRequestOrderBy.Published,
                    Sort = null
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Expect data set files belonging to the newest published release to be returned first
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
            public async Task OrderByRelevanceAscending_Success()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(releaseFilesRelease2[1].Id, 1),
                    new(releaseFilesRelease1[1].Id, 2),
                    new(releaseFilesRelease1[0].Id, 3),
                    new(releaseFilesRelease2[0].Id, 4)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = new Mock<ContentDbContext>();
                contentDbContext.Setup(context => context.ReleaseFiles)
                    .Returns(allReleaseFiles.AsQueryable().BuildMockDbSet().Object);
                contentDbContext.Setup(context => context.ReleaseFilesFreeTextTable("aaa"))
                    .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    SearchTerm = "aaa",
                    OrderBy = DataSetsListRequestOrderBy.Relevance,
                    Sort = SortOrder.Asc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease2[1],
                    releaseFilesRelease1[1],
                    releaseFilesRelease1[0],
                    releaseFilesRelease2[0]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByRelevanceDescending_Success()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(releaseFilesRelease2[1].Id, 1),
                    new(releaseFilesRelease1[1].Id, 2),
                    new(releaseFilesRelease1[0].Id, 3),
                    new(releaseFilesRelease2[0].Id, 4)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = new Mock<ContentDbContext>();
                contentDbContext.Setup(context => context.ReleaseFiles)
                    .Returns(allReleaseFiles.AsQueryable().BuildMockDbSet().Object);
                contentDbContext.Setup(context => context.ReleaseFilesFreeTextTable("aaa"))
                    .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    SearchTerm = "aaa",
                    OrderBy = DataSetsListRequestOrderBy.Relevance,
                    Sort = SortOrder.Desc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease2[0],
                    releaseFilesRelease1[0],
                    releaseFilesRelease1[1],
                    releaseFilesRelease2[1]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByRelevanceWithoutSortOrder_DefaultsToDescending()
            {
                var (_, _, _, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(releaseFilesRelease2[1].Id, 1),
                    new(releaseFilesRelease1[1].Id, 2),
                    new(releaseFilesRelease1[0].Id, 3),
                    new(releaseFilesRelease2[0].Id, 4)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = new Mock<ContentDbContext>();
                contentDbContext.Setup(context => context.ReleaseFiles)
                    .Returns(allReleaseFiles.AsQueryable().BuildMockDbSet().Object);
                contentDbContext.Setup(context => context.ReleaseFilesFreeTextTable("aaa"))
                    .Returns(freeTextRanks.AsQueryable().BuildMockDbSet().Object);

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    SearchTerm = "aaa",
                    OrderBy = DataSetsListRequestOrderBy.Relevance,
                    Sort = null
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease2[0],
                    releaseFilesRelease1[0],
                    releaseFilesRelease1[1],
                    releaseFilesRelease2[1]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }
        }

        public class SupersededPublicationTests : ListDataSetsTests
        {
            public SupersededPublicationTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Fact]
            public async Task PublicationIsSuperseded_DataSetsOfSupersededPublicationsAreExcluded()
            {
                var (release1, release2, _, _, releaseFilesRelease2, allReleaseFiles) = GenerateTestData();

                // Set the publication of release 1 to be superseded by the publication of release 2
                release1.Publication.SupersededById = release2.PublicationId;

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Expect all data set files belonging to release 1 to be excluded as it is superseded
                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease2[0],
                    releaseFilesRelease2[1]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterBySupersededPublicationId_SupersededStatusIsIgnored()
            {
                var (release1, release2, _, releaseFilesRelease1, _, allReleaseFiles) = GenerateTestData();

                // Set the publication of release 1 to be superseded by the publication of release 2
                release1.Publication.SupersededById = release2.PublicationId;

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    PublicationId = release1.PublicationId
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Filtering by a publication id should ignore the superseded status and return all data set files
                // belonging to the publication
                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    releaseFilesRelease1[0],
                    releaseFilesRelease1[1]
                };

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task SupersedingPublicationHasNoPublishedReleases_SupersededStatusIsIgnored()
            {
                var (release1, _, unpublishedRelease, releaseFilesRelease1, releaseFilesRelease2, allReleaseFiles) =
                    GenerateTestData();

                // Set the publication of release 1 to be superseded by a publication with no published releases
                release1.Publication.SupersededById = unpublishedRelease.PublicationId;

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // The superseded status should be ignored as the superseding publication has no published releases
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
        }

        public class ValidationTests : ListDataSetsTests
        {
            public ValidationTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task PageTooSmall_ReturnsBadRequest(int page)
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(
                    Page: page
                );
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasGreaterThanOrEqualError("page");
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            public async Task PageSizeTooSmall_ReturnsBadRequest(int pageSize)
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(
                    PageSize: pageSize
                );
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasGreaterThanOrEqualError("pageSize");
            }

            [Theory]
            [InlineData(41)]
            public async Task PageSizeTooBig_ReturnsBadRequest(int pageSize)
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(
                    PageSize: pageSize
                );
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasLessThanOrEqualError("pageSize");
            }

            [Theory]
            [InlineData("a")]
            [InlineData("aa")]
            public async Task SearchTermTooSmall_ReturnsBadRequest(string searchTerm)
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(
                    SearchTerm: searchTerm
                );
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasMinimumLengthError("searchTerm");
            }

            [Fact]
            public async Task OrderByNaturalWithoutReleaseId_ReturnsBadRequest()
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(
                    OrderBy: DataSetsListRequestOrderBy.Natural
                );
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasNotEmptyError("releaseId");
            }

            [Fact]
            public async Task OrderByRelevanceWithoutSearchTerm_ReturnsBadRequest()
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(
                    OrderBy: DataSetsListRequestOrderBy.Relevance
                );
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasNotEmptyError("searchTerm");
            }
        }

        public class MiscellaneousTests : ListDataSetsTests
        {
            public MiscellaneousTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Fact]
            public async Task NoPublishedDataSets_ReturnsEmpty()
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                AssertPaginatedViewModel(pagedResult, expectedTotalResults: 0);
                Assert.Empty(pagedResult.Results);
            }

            [Fact]
            // TODO Remove this once we do further work to remove all HTML from summaries at source
            public async Task ReleaseFileSummariesContainHtml_HtmlTagsAreStripped()
            {
                var (_, _, _, _, _, allReleaseFiles) = GenerateTestData();

                allReleaseFiles.ForEach(releaseFile => { releaseFile.Summary = $"<p>{releaseFile.Summary}</p>"; });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.AddRange(allReleaseFiles))
                    .CreateClient();

                var query = new DataSetsListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                Assert.All(pagedResult.Results, item =>
                {
                    var content = item.Content;
                    Assert.DoesNotContain("<p>", content);
                    Assert.DoesNotContain("</p>", content);
                });
            }
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
            Release release1,
            Release release2,
            Release unpublishedRelease,
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

            return (release1,
                release2,
                unpublishedRelease,
                releaseFilesRelease1,
                releaseFilesRelease2,
                allReleaseFiles);
        }
    }

    private WebApplicationFactory<TestStartup> BuildApp(IDataSetService? dataSetService = null)
    {
        return TestApp
            .ResetDbContexts()
            .ConfigureServices(services =>
            {
                services.AddTransient(
                    s => dataSetService ?? new DataSetService(s.GetRequiredService<ContentDbContext>()));
            });
    }
}
