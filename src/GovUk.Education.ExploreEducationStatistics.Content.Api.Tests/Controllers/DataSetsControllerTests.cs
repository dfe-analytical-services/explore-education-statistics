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

    private DataSetsControllerTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
    {
    }

    public class ListDataSetsTests : DataSetsControllerTests
    {
        private ListDataSetsTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
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
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Publications each have a published release version
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetsForRelease(publication1.Releases[0]);
                var publication2Release1Version1Files = GenerateDataSetsForRelease(publication2.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest(ReleaseId: publication1.Releases[0].Id);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: publication1Release1Version1Files.Count);
                AssertResultsForExpectedReleaseFiles(publication1Release1Version1Files, pagedResult.Results);
            }

            [Fact]
            public async Task FilterByPublicationId_Success()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Publications each have a published release version
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetsForRelease(publication1.Releases[0]);
                var publication2Release1Version1Files = GenerateDataSetsForRelease(publication2.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest(PublicationId: publication1.Id);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: publication1Release1Version1Files.Count);
                AssertResultsForExpectedReleaseFiles(publication1Release1Version1Files, pagedResult.Results);
            }

            [Fact]
            public async Task FilterByThemeId_Success()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Publications each have a published release version
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    // Publications have different themes
                    .WithTopics(_fixture.DefaultTopic()
                        .WithThemes(_fixture.DefaultTheme()
                            .Generate(2))
                        .Generate(2))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetsForRelease(publication1.Releases[0]);
                var publication2Release1Version1Files = GenerateDataSetsForRelease(publication2.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest(ThemeId: publication1.Topic.ThemeId);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: publication1Release1Version1Files.Count);
                AssertResultsForExpectedReleaseFiles(publication1Release1Version1Files, pagedResult.Results);
            }

            [Fact]
            public async Task FilterByReleaseIdWhereReleaseIsUnpublished_ReturnsEmpty()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Index 0 has a published and unpublished release version
                    // Index 1 has a published release version
                    .ForIndex(0, p => p.SetReleaseParents(_fixture
                        .DefaultReleaseParent(publishedVersions: 1, draftVersion: true)
                        .Generate(1)))
                    .ForIndex(1, p => p.SetReleaseParents(_fixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1)))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetsForRelease(publication1.Releases[0]);
                var publication1Release1Version2Files = GenerateDataSetsForRelease(publication1.Releases[1]);
                var publication2Release1Version1Files = GenerateDataSetsForRelease(publication2.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication1Release1Version2Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest(ReleaseId: publication1.Releases[1].Id);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Fact]
            public async Task FilterByPublicationIdWherePublicationIsUnpublished_ReturnsEmpty()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Index 0 has an unpublished release version
                    // Index 1 has a published release version
                    .ForIndex(0, p => p.SetReleaseParents(_fixture
                        .DefaultReleaseParent(publishedVersions: 0, draftVersion: true)
                        .Generate(1)))
                    .ForIndex(1, p => p.SetReleaseParents(_fixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1)))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetsForRelease(publication1.Releases[0]);
                var publication2Release1Version1Files = GenerateDataSetsForRelease(publication2.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest(PublicationId: publication1.Id);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Fact]
            public async Task FilterByThemeIdWhereThemeIsUnpublished_ReturnsEmpty()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Index 0 has an unpublished release version
                    // Index 1 has a published release version
                    .ForIndex(0, p => p.SetReleaseParents(_fixture
                        .DefaultReleaseParent(publishedVersions: 0, draftVersion: true)
                        .Generate(1)))
                    .ForIndex(1, p => p.SetReleaseParents(_fixture
                        .DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1)))
                    // Publications have different themes
                    .WithTopics(_fixture.DefaultTopic()
                        .WithThemes(_fixture.DefaultTheme()
                            .Generate(2))
                        .Generate(2))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetsForRelease(publication1.Releases[0]);
                var publication2Release1Version1Files = GenerateDataSetsForRelease(publication2.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest(ThemeId: publication1.Topic.ThemeId);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Fact]
            public async Task FilterBySearchTerm_Success()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetsForRelease(publication.Releases[0]);

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(release1Version1Files[0].Id, 1),
                    new(release1Version1Files[1].Id, 2)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = ContentDbContextMock(
                    release1Version1Files,
                    freeTextRanks);

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest(SearchTerm: "aaa");
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    release1Version1Files[1],
                    release1Version1Files[0]
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterBySearchTermWhereTermIsNotFound_ReturnsEmpty()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetsForRelease(publication.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = ContentDbContextMock(release1Version1Files);

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest(SearchTerm: "aaa");
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Fact]
            public async Task NoFilter_ReturnsAllResultsOrderedByTitleAscending()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Publications each have a published release version
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetsForRelease(publication1.Releases[0]);
                var publication2Release1Version1Files = GenerateDataSetsForRelease(publication2.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = publication1Release1Version1Files
                    .Concat(publication2Release1Version1Files)
                    .OrderBy(file => file.Name)
                    .ToList();

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }
        }

        public class OrderByTests : ListDataSetsTests
        {
            public OrderByTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Theory]
            [InlineData(SortOrder.Asc)]
            [InlineData(null)]
            public async Task OrderByTitle_OrdersByTitleInAscendingSortOrderAndIsAscendingByDefault(
                SortOrder? sortOrder)
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetsForRelease(publication.Releases[0]);

                // Apply a descending sequence of titles to the data set files
                release1Version1Files[0].Name = "b";
                release1Version1Files[1].Name = "a";

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    OrderBy = DataSetsListRequestOrderBy.Title,
                    Sort = sortOrder
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    release1Version1Files[1], // Has title "a"
                    release1Version1Files[0], // Has title "b"
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByTitleDescending_OrdersByTitleInDescendingSortOrder()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetsForRelease(publication.Releases[0]);

                // Apply an ascending sequence of titles to the data set files
                release1Version1Files[0].Name = "a";
                release1Version1Files[1].Name = "b";

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
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
                    release1Version1Files[1], // Has title "b"
                    release1Version1Files[0], // Has title "a"
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Theory]
            [InlineData(SortOrder.Asc)]
            [InlineData(null)]
            public async Task OrderByNatural_OrdersByNaturalInAscendingSortOrderAndIsAscendingByDefault(
                SortOrder? sortOrder)
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetsForRelease(publication.Releases[0]);

                // Apply a descending natural order to the data set files
                release1Version1Files[0].Order = 1;
                release1Version1Files[1].Order = 0;

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    ReleaseId = publication.Releases[0].Id,
                    OrderBy = DataSetsListRequestOrderBy.Natural,
                    Sort = sortOrder
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Expect data set files to be returned in ascending natural order
                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    release1Version1Files[1], // Has natural order 0
                    release1Version1Files[0], // Has natural order 1
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByNaturalDescending_OrdersByNaturalInDescendingSortOrder()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetsForRelease(publication.Releases[0]);

                // Apply an ascending natural order to the data set files
                release1Version1Files[0].Order = 0;
                release1Version1Files[1].Order = 1;

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    ReleaseId = publication.Releases[0].Id,
                    OrderBy = DataSetsListRequestOrderBy.Natural,
                    Sort = SortOrder.Desc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Expect data set files to be returned in descending natural order
                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    release1Version1Files[1], // Has natural order 1
                    release1Version1Files[0], // Has natural order 0
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByPublishedAscending_OrdersByPublishedInAscendingSortOrder()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Publications each have a published release version
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1 = publication1.Releases[0];
                var publication2Release1Version1 = publication2.Releases[0];

                // Apply a descending sequence of published dates to the releases
                publication1Release1Version1.Published = DateTime.UtcNow.AddDays(-1);
                publication2Release1Version1.Published = DateTime.UtcNow.AddDays(-2);

                var publication1Release1Version1Files = GenerateDataSetsForRelease(publication1Release1Version1);
                var publication2Release1Version1Files = GenerateDataSetsForRelease(publication2Release1Version1);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
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
                    publication2Release1Version1Files[0], // Published 2 days ago
                    publication2Release1Version1Files[1], // Published 2 days ago
                    publication1Release1Version1Files[0], // Published 1 day ago
                    publication1Release1Version1Files[1], // Published 1 day ago
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Theory]
            [InlineData(SortOrder.Desc)]
            [InlineData(null)]
            public async Task OrderByPublished_OrdersByPublishedInDescendingSortOrderAndIsDescendingByDefault(
                SortOrder? sortOrder)
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Publications each have a published release version
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1 = publication1.Releases[0];
                var publication2Release1Version1 = publication2.Releases[0];

                // Apply an ascending sequence of published dates to the releases
                publication1Release1Version1.Published = DateTime.UtcNow.AddDays(-2);
                publication2Release1Version1.Published = DateTime.UtcNow.AddDays(-1);

                var publication1Release1Version1Files = GenerateDataSetsForRelease(publication1Release1Version1);
                var publication2Release1Version1Files = GenerateDataSetsForRelease(publication2Release1Version1);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    OrderBy = DataSetsListRequestOrderBy.Published,
                    Sort = sortOrder
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Expect data set files belonging to the newest published release to be returned first
                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    publication2Release1Version1Files[0], // Published 1 day ago
                    publication2Release1Version1Files[1], // Published 1 day ago
                    publication1Release1Version1Files[0], // Published 2 days ago
                    publication1Release1Version1Files[1], // Published 2 days ago
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task OrderByRelevanceAscending_OrdersByRelevanceInAscendingSortOrder()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetsForRelease(publication.Releases[0], numberOfDataSets: 3);

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(release1Version1Files[0].Id, 2),
                    new(release1Version1Files[1].Id, 3),
                    new(release1Version1Files[2].Id, 1)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = ContentDbContextMock(
                    release1Version1Files,
                    freeTextRanks);

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
                    release1Version1Files[2], // Has rank 1
                    release1Version1Files[0], // Has rank 2
                    release1Version1Files[1], //  Has rank 3
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Theory]
            [InlineData(SortOrder.Desc)]
            [InlineData(null)]
            public async Task OrderByRelevance_OrdersByRelevanceInDescendingSortOrderAndIsDescendingByDefault(
                SortOrder? sortOrder)
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetsForRelease(publication.Releases[0], numberOfDataSets: 3);

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(release1Version1Files[0].Id, 2),
                    new(release1Version1Files[1].Id, 3),
                    new(release1Version1Files[2].Id, 1)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = ContentDbContextMock(
                    release1Version1Files,
                    freeTextRanks);

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest
                {
                    SearchTerm = "aaa",
                    OrderBy = DataSetsListRequestOrderBy.Relevance,
                    Sort = sortOrder
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

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

        public class SupersededPublicationTests : ListDataSetsTests
        {
            public SupersededPublicationTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Fact]
            public async Task PublicationIsSuperseded_DataSetsOfSupersededPublicationsAreExcluded()
            {
                Publication supersedingPublication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                Publication supersededPublication = _fixture
                    .DefaultPublication()
                    .WithSupersededBy(supersedingPublication)
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var supersedingPublicationReleaseFiles = GenerateDataSetsForRelease(supersedingPublication.Releases[0]);
                var supersededPublicationReleaseFiles = GenerateDataSetsForRelease(supersededPublication.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(supersedingPublicationReleaseFiles);
                        context.ReleaseFiles.AddRange(supersededPublicationReleaseFiles);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Expect all data set files of the superseded publication to be excluded and only the data set files
                // of the superseding publication to be returned

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: supersedingPublicationReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(supersedingPublicationReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterBySupersededPublicationId_SupersededStatusIsIgnored()
            {
                Publication supersedingPublication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                Publication supersededPublication = _fixture
                    .DefaultPublication()
                    .WithSupersededBy(supersedingPublication)
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var supersedingPublicationReleaseFiles = GenerateDataSetsForRelease(supersedingPublication.Releases[0]);
                var supersededPublicationReleaseFiles = GenerateDataSetsForRelease(supersededPublication.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(supersedingPublicationReleaseFiles);
                        context.ReleaseFiles.AddRange(supersededPublicationReleaseFiles);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest(PublicationId: supersededPublication.Id);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // Filtering by the superseded publication id should ignore the superseded status and return all data
                // set files belonging to the publication

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: supersededPublicationReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(supersededPublicationReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task SupersedingPublicationHasNoPublishedReleases_SupersededStatusIsIgnored()
            {
                Publication supersedingPublication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 0, draftVersion: true)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                Publication supersededPublication = _fixture
                    .DefaultPublication()
                    .WithSupersededBy(supersedingPublication)
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var supersedingPublicationReleaseFiles = GenerateDataSetsForRelease(supersedingPublication.Releases[0]);
                var supersededPublicationReleaseFiles = GenerateDataSetsForRelease(supersededPublication.Releases[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(supersedingPublicationReleaseFiles);
                        context.ReleaseFiles.AddRange(supersededPublicationReleaseFiles);
                    })
                    .CreateClient();

                var query = new DataSetsListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                // The superseded status should be ignored as the superseding publication has no published releases
                // and all data set files belonging to the superseded publication should be returned

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: supersededPublicationReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(supersededPublicationReleaseFiles, pagedResult.Results);
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
            [InlineData(10000)]
            public async Task PageOutsideAllowedRange_ReturnsValidationError(int page)
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(Page: page);
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasInclusiveBetweenError("page");
            }

            [Theory]
            [InlineData(1)]
            [InlineData(9999)]
            public async Task PageInAllowedRange_Success(int page)
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .CreateClient();

                var query = new DataSetsListRequest(Page: page);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults(expectedPage: page);
            }

            [Theory]
            [InlineData(-1)]
            [InlineData(0)]
            [InlineData(41)]
            public async Task PageSizeOutsideAllowedRange_ReturnsValidationError(int pageSize)
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(PageSize: pageSize);
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasInclusiveBetweenError("pageSize");
            }

            [Theory]
            [InlineData(1)]
            [InlineData(40)]
            public async Task PageSizeInAllowedRange_Success(int pageSize)
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .CreateClient();

                var query = new DataSetsListRequest(PageSize: pageSize);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults(expectedPageSize: pageSize);
            }

            [Theory]
            [InlineData("a")]
            [InlineData("aa")]
            public async Task SearchTermBelowMinimumLength_ReturnsValidationError(string searchTerm)
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(SearchTerm: searchTerm);
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasMinimumLengthError("searchTerm");
            }

            [Theory]
            [InlineData("aaa")]
            [InlineData("aaaa")]
            public async Task SearchTermAboveMinimumLength_Success(string searchTerm)
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = ContentDbContextMock();

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest(SearchTerm: searchTerm);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();
            }

            [Fact]
            public async Task OrderByNaturalWithoutReleaseId_ReturnsValidationError()
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(
                    ReleaseId: null,
                    OrderBy: DataSetsListRequestOrderBy.Natural
                );
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasNotEmptyError("releaseId");
            }

            [Fact]
            public async Task OrderByNaturalWithReleaseId_Success()
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(
                    ReleaseId: Guid.NewGuid(),
                    OrderBy: DataSetsListRequestOrderBy.Natural
                );
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();
            }

            [Fact]
            public async Task OrderByRelevanceWithoutSearchTerm_ReturnsValidationError()
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetsListRequest(
                    SearchTerm: null,
                    OrderBy: DataSetsListRequestOrderBy.Relevance
                );
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasNotEmptyError("searchTerm");
            }

            [Fact]
            public async Task OrderByRelevanceWithSearchTerm_Success()
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var contentDbContext = ContentDbContextMock();

                var client = BuildApp(
                        dataSetService: new DataSetService(contentDbContext.Object))
                    .CreateClient();

                var query = new DataSetsListRequest(
                    SearchTerm: "aaa",
                    OrderBy: DataSetsListRequestOrderBy.Relevance
                );
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                response.AssertOk<PaginatedListViewModel<DataSetListViewModel>>();
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

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Fact]
            // TODO Remove this once we do further work to remove all HTML from summaries at source
            public async Task ReleaseFileSummariesContainHtml_HtmlTagsAreStripped()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleaseParents(_fixture.DefaultReleaseParent(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetsForRelease(publication.Releases[0]);

                release1Version1Files.ForEach(releaseFile =>
                {
                    releaseFile.Summary = $"<p>{releaseFile.Summary}</p>";
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetsCacheKey, PaginatedListViewModel<DataSetListViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
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
                { "latestOnly", request.LatestOnly?.ToString() },
                { "searchTerm", request.SearchTerm },
                { "orderBy", request.OrderBy?.ToString() },
                { "sort", request.Sort?.ToString() },
                { "page", request.Page.ToString() },
                { "pageSize", request.PageSize.ToString() }
            };

            var uri = QueryHelpers.AddQueryString("/api/data-sets", queryParams);

            return await client.GetAsync(uri);
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
                    () => Assert.Equal(release.Id == publication.LatestPublishedReleaseId, viewModel.LatestData),
                    () => Assert.Equal(releaseFile.Release.Published!.Value, viewModel.Published)
                );
            });
        }

        private List<ReleaseFile> GenerateDataSetsForRelease(Release release, int numberOfDataSets = 2)
        {
            return _fixture.DefaultReleaseFile()
                .WithRelease(release)
                .WithFiles(_fixture.DefaultFile()
                    .GenerateList(numberOfDataSets))
                .GenerateList();
        }

        private static Mock<ContentDbContext> ContentDbContextMock(
            IEnumerable<ReleaseFile>? releaseFiles = null,
            IEnumerable<FreeTextRank>? freeTextRanks = null)
        {
            var contentDbContext = new Mock<ContentDbContext>();
            contentDbContext.Setup(context => context.ReleaseFiles)
                .Returns((releaseFiles ?? Array.Empty<ReleaseFile>()).AsQueryable().BuildMockDbSet().Object);
            contentDbContext.Setup(context => context.ReleaseFilesFreeTextTable(It.IsAny<string>()))
                .Returns((freeTextRanks ?? Array.Empty<FreeTextRank>()).AsQueryable().BuildMockDbSet().Object);
            return contentDbContext;
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
