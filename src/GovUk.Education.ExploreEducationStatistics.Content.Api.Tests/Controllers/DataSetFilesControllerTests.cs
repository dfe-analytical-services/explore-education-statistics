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
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class DataSetFilesControllerTests : IntegrationTest<TestStartup>
{
    private readonly DataFixture _fixture = new();

    private DataSetFilesControllerTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
    {
    }

    public class ListDataSetFilesTests : DataSetFilesControllerTests
    {
        private ListDataSetFilesTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
        {
        }

        public class FilterTests : ListDataSetFilesTests
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
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[0]);
                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest(ReleaseId: publication1.ReleaseVersions[0].Id);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

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
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[0]);
                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest(PublicationId: publication1.Id);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

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
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    // Publications have different themes
                    .WithTopics(_fixture.DefaultTopic()
                        .WithThemes(_fixture.DefaultTheme()
                            .Generate(2))
                        .Generate(2))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[0]);
                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest(ThemeId: publication1.Topic.ThemeId);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: publication1Release1Version1Files.Count);
                AssertResultsForExpectedReleaseFiles(publication1Release1Version1Files, pagedResult.Results);
            }

            [Theory]
            [InlineData(false)]
            [InlineData(null)]
            public async Task FilterByReleaseIdWhereReleaseIsNotLatestForPublication_Success(bool? latestOnly)
            {
                // Set up a publication with 2 releases each with a published version.
                // The 2021/22 Academic year will be the latest release in reverse chronological order.
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_ => ListOf<Release>(
                        _fixture
                            .DefaultRelease(publishedVersions: 1, year: 2021),
                        _fixture
                            .DefaultRelease(publishedVersions: 1, year: 2020)))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);
                var release2Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[1]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(release1Version1Files);
                        context.ReleaseFiles.AddRange(release2Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[1].Id,
                    LatestOnly = latestOnly
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                // Expect data set files of an old release to be returned when LatestOnly is false or null
                // (LatestOnly should default to false when a releaseId is specified).
                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: release2Version1Files.Count);
                AssertResultsForExpectedReleaseFiles(release2Version1Files, pagedResult.Results);
            }

            [Fact]
            public async Task FilterByReleaseIdWhereReleaseIsNotLatestForPublicationAndLatestOnlyTrue_ReturnsEmpty()
            {
                // Set up a publication with 2 releases each with a published version.
                // The 2021/22 Academic year will be the latest release in reverse chronological order.
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_ => ListOf<Release>(
                        _fixture
                            .DefaultRelease(publishedVersions: 1, year: 2021),
                        _fixture
                            .DefaultRelease(publishedVersions: 1, year: 2020)))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);
                var release2Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[1]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(release1Version1Files);
                        context.ReleaseFiles.AddRange(release2Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[1].Id,
                    LatestOnly = true
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                // Expect no data set files to be returned for an old release when LatestOnly is true
                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            [InlineData(null)]
            public async Task FilterByReleaseIdWhereReleaseIsUnpublished_ReturnsEmpty(bool? latestOnly)
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Index 0 has a published and unpublished release version
                    // Index 1 has a published release version
                    .ForIndex(0, p => p.SetReleases(_fixture
                        .DefaultRelease(publishedVersions: 1, draftVersion: true)
                        .Generate(1)))
                    .ForIndex(1, p => p.SetReleases(_fixture
                        .DefaultRelease(publishedVersions: 1)
                        .Generate(1)))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[0]);
                var publication1Release1Version2Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[1]);
                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication1Release1Version2Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication1.ReleaseVersions[1].Id,
                    LatestOnly = latestOnly
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                // Expect no data set files to be returned for an unpublished release version regardless of LatestOnly
                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            [InlineData(null)]
            public async Task FilterByReleaseIdWhereReleaseIsNotLatestVersion_ReturnsEmpty(bool? latestOnly)
            {
                // Set up a publication with a release that has 2 published versions
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture
                        .DefaultRelease(publishedVersions: 2)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);
                var release1Version2Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[1]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(release1Version1Files);
                        context.ReleaseFiles.AddRange(release1Version2Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[0].Id,
                    LatestOnly = latestOnly
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                // Expect no data set files to be returned unless they are associated with a latest published release
                // version regardless of LatestOnly
                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Fact]
            public async Task FilterByPublicationIdWherePublicationIsUnpublished_ReturnsEmpty()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Index 0 has an unpublished release version
                    // Index 1 has a published release version
                    .ForIndex(0, p => p.SetReleases(_fixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1)))
                    .ForIndex(1, p => p.SetReleases(_fixture
                        .DefaultRelease(publishedVersions: 1)
                        .Generate(1)))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[0]);
                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest(PublicationId: publication1.Id);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Fact]
            public async Task FilterByThemeIdWhereThemeIsUnpublished_ReturnsEmpty()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Index 0 has an unpublished release version
                    // Index 1 has a published release version
                    .ForIndex(0, p => p.SetReleases(_fixture
                        .DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1)))
                    .ForIndex(1, p => p.SetReleases(_fixture
                        .DefaultRelease(publishedVersions: 1)
                        .Generate(1)))
                    // Publications have different themes
                    .WithTopics(_fixture.DefaultTopic()
                        .WithThemes(_fixture.DefaultTheme()
                            .Generate(2))
                        .Generate(2))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[0]);
                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest(ThemeId: publication1.Topic.ThemeId);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Fact]
            public async Task FilterBySearchTerm_Success()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(release1Version1Files[0].Id, 1),
                    new(release1Version1Files[1].Id, 2)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var contentDbContext = ContentDbContextMock(
                    publication.ReleaseVersions,
                    release1Version1Files,
                    freeTextRanks);

                var client = BuildApp(contentDbContext.Object)
                    .CreateClient();

                var query = new DataSetFileListRequest(SearchTerm: "aaa");
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

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
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var contentDbContext = ContentDbContextMock(
                    publication.ReleaseVersions,
                    release1Version1Files);

                var client = BuildApp(contentDbContext.Object)
                    .CreateClient();

                var query = new DataSetFileListRequest(SearchTerm: "aaa");
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Theory]
            [InlineData(true)]
            [InlineData(null)]
            public async Task FilterByLatestOnly_ReturnsDataSetsFromLatestPublishedReleaseVersionOfPublications(
                bool? latestOnly)
            {
                // Set up 2 publications where both have multiple releases each with a mix of published and
                // unpublished versions. The 2021/22 Academic year will be the latest release in
                // reverse chronological order with a published release version for both publications.
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    .WithReleases(_ => ListOf<Release>(
                        _fixture
                            .DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2022),
                        _fixture
                            .DefaultRelease(publishedVersions: 2, draftVersion: true, year: 2021),
                        _fixture
                            .DefaultRelease(publishedVersions: 1, year: 2020)))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[0]);
                var publication1Release2Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[1]);
                var publication1Release2Version2Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[2]);
                var publication1Release2Version3Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[3]);
                var publication1Release3Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[4]);

                var publication1ReleaseFiles = publication1Release1Version1Files
                    .Concat(publication1Release2Version1Files)
                    .Concat(publication1Release2Version2Files)
                    .Concat(publication1Release2Version3Files)
                    .Concat(publication1Release3Version1Files)
                    .ToList();

                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[0]);
                var publication2Release2Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[1]);
                var publication2Release2Version2Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[2]);
                var publication2Release2Version3Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[3]);
                var publication2Release3Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[4]);

                var publication2ReleaseFiles = publication2Release1Version1Files
                    .Concat(publication2Release2Version1Files)
                    .Concat(publication2Release2Version2Files)
                    .Concat(publication2Release2Version3Files)
                    .Concat(publication2Release3Version1Files)
                    .ToList();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1ReleaseFiles);
                        context.ReleaseFiles.AddRange(publication2ReleaseFiles);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest(LatestOnly: latestOnly);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                // Expect the result to be the data set files of the latest published release versions
                // of the latest published releases of both publications, in ascending title order
                var expectedReleaseFiles = publication1Release2Version2Files
                    .Concat(publication2Release2Version2Files)
                    .OrderBy(rf => rf.Name)
                    .ToList();

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterByLatestOnlyFalse_ReturnsDataSetsFromAnyLatestPublishedReleaseVersions()
            {
                // Set up 2 publications where both have multiple releases each with a mix of published and
                // unpublished versions
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    .WithReleases(_ => ListOf<Release>(
                        _fixture
                            .DefaultRelease(publishedVersions: 0, draftVersion: true),
                        _fixture
                            .DefaultRelease(publishedVersions: 2, draftVersion: true),
                        _fixture
                            .DefaultRelease(publishedVersions: 1)))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[0]);
                var publication1Release2Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[1]);
                var publication1Release2Version2Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[2]);
                var publication1Release2Version3Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[3]);
                var publication1Release3Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[4]);

                var publication1ReleaseFiles = publication1Release1Version1Files
                    .Concat(publication1Release2Version1Files)
                    .Concat(publication1Release2Version2Files)
                    .Concat(publication1Release2Version3Files)
                    .Concat(publication1Release3Version1Files)
                    .ToList();

                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[0]);
                var publication2Release2Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[1]);
                var publication2Release2Version2Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[2]);
                var publication2Release2Version3Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[3]);
                var publication2Release3Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[4]);

                var publication2ReleaseFiles = publication2Release1Version1Files
                    .Concat(publication2Release2Version1Files)
                    .Concat(publication2Release2Version2Files)
                    .Concat(publication2Release2Version3Files)
                    .Concat(publication2Release3Version1Files)
                    .ToList();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1ReleaseFiles);
                        context.ReleaseFiles.AddRange(publication2ReleaseFiles);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest(LatestOnly: false);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                // Expect the result to be the data set files of the latest published release versions
                // of both publications, in ascending title order
                var expectedReleaseFiles = publication1Release2Version2Files
                    .Concat(publication1Release3Version1Files)
                    .Concat(publication2Release2Version2Files)
                    .Concat(publication2Release3Version1Files)
                    .OrderBy(rf => rf.Name)
                    .ToList();

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task FilterByDataSetType_Api_ReturnsOnlyDataSetsWithAssociatedApiDataSets()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture
                        .DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var releaseVersionFiles = _fixture.DefaultReleaseFile()
                    .WithReleaseVersion(publication.ReleaseVersions[0])
                    .WithFiles(_fixture.DefaultFile()
                        .ForIndex(0, s => s.SetPublicDataSetVersionId(Guid.NewGuid()))
                        .ForIndex(1, s => s.SetPublicDataSetVersionId(Guid.NewGuid()))
                        .GenerateList(5))
                    .GenerateList();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(releaseVersionFiles))
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    DataSetType = DataSetType.Api
                };

                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var expectedReleaseFiles = releaseVersionFiles
                    .Where(rf => rf.File.PublicDataSetVersionId.HasValue)
                    .OrderBy(rf => rf.Name)
                    .ToList();

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Theory]
            [InlineData(DataSetType.All)]
            [InlineData(null)]
            public async Task FilterByDataSetType_AllOrUnset_ReturnsAllDataSets(DataSetType? dataSetType)
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture
                        .DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var releaseVersionFiles = _fixture.DefaultReleaseFile()
                    .WithReleaseVersion(publication.ReleaseVersions[0])
                    .WithFiles(_fixture.DefaultFile()
                        .ForIndex(0, s => s.SetPublicDataSetVersionId(Guid.NewGuid()))
                        .ForIndex(1, s => s.SetPublicDataSetVersionId(Guid.NewGuid()))
                        .GenerateList(5))
                    .GenerateList();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(releaseVersionFiles))
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    DataSetType = dataSetType
                };

                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: releaseVersionFiles.Count);
                AssertResultsForExpectedReleaseFiles(releaseVersionFiles, pagedResult.Results);
            }

            [Fact]
            public async Task NoFilter_ReturnsAllResultsOrderedByTitleAscending()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Publications each have a published release version
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1.ReleaseVersions[0]);
                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var expectedReleaseFiles = publication1Release1Version1Files
                    .Concat(publication2Release1Version1Files)
                    .OrderBy(file => file.Name)
                    .ToList();

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }
        }

        public class SortByTests : ListDataSetFilesTests
        {
            public SortByTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Theory]
            [InlineData(SortDirection.Asc)]
            [InlineData(null)]
            public async Task SortByTitle_SortsByTitleInAscendingOrderAndIsAscendingByDefault(
                SortDirection? sortDirection)
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);

                // Apply a descending sequence of titles to the data set files
                release1Version1Files[0].Name = "b";
                release1Version1Files[1].Name = "a";

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    Sort = DataSetsListRequestSortBy.Title,
                    SortDirection = sortDirection
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    release1Version1Files[1], // Has title "a"
                    release1Version1Files[0], // Has title "b"
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Fact]
            public async Task SortByTitleDescending_SortsByTitleInDescendingOrder()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);

                // Apply an ascending sequence of titles to the data set files
                release1Version1Files[0].Name = "a";
                release1Version1Files[1].Name = "b";

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    Sort = DataSetsListRequestSortBy.Title,
                    SortDirection = SortDirection.Desc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var expectedReleaseFiles = new List<ReleaseFile>
                {
                    release1Version1Files[1], // Has title "b"
                    release1Version1Files[0], // Has title "a"
                };

                pagedResult.AssertHasExpectedPagingAndResultCount(expectedTotalResults: expectedReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(expectedReleaseFiles, pagedResult.Results);
            }

            [Theory]
            [InlineData(SortDirection.Asc)]
            [InlineData(null)]
            public async Task SortByNatural_SortsByNaturalInAscendingOrderAndIsAscendingByDefault(
                SortDirection? sortDirection)
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);

                // Apply a descending natural order to the data set files
                release1Version1Files[0].Order = 1;
                release1Version1Files[1].Order = 0;

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[0].Id,
                    Sort = DataSetsListRequestSortBy.Natural,
                    SortDirection = sortDirection
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

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
            public async Task SortByNaturalDescending_SortsByNaturalInDescendingOrder()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);

                // Apply an ascending natural order to the data set files
                release1Version1Files[0].Order = 0;
                release1Version1Files[1].Order = 1;

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[0].Id,
                    Sort = DataSetsListRequestSortBy.Natural,
                    SortDirection = SortDirection.Desc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

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
            public async Task SortByPublishedAscending_SortsByPublishedInAscendingOrder()
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Publications each have a published release version
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1 = publication1.ReleaseVersions[0];
                var publication2Release1Version1 = publication2.ReleaseVersions[0];

                // Apply a descending sequence of published dates to the releases
                publication1Release1Version1.Published = DateTime.UtcNow.AddDays(-1);
                publication2Release1Version1.Published = DateTime.UtcNow.AddDays(-2);

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1Release1Version1);
                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2Release1Version1);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    Sort = DataSetsListRequestSortBy.Published,
                    SortDirection = SortDirection.Asc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

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
            [InlineData(SortDirection.Desc)]
            [InlineData(null)]
            public async Task SortByPublished_SortsByPublishedInDescendingOrderAndIsDescendingByDefault(
                SortDirection? sortDirection)
            {
                var (publication1, publication2) = _fixture
                    .DefaultPublication()
                    // Publications each have a published release version
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate(2)
                    .ToTuple2();

                var publication1Release1Version1 = publication1.ReleaseVersions[0];
                var publication2Release1Version1 = publication2.ReleaseVersions[0];

                // Apply an ascending sequence of published dates to the releases
                publication1Release1Version1.Published = DateTime.UtcNow.AddDays(-2);
                publication2Release1Version1.Published = DateTime.UtcNow.AddDays(-1);

                var publication1Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication1Release1Version1);
                var publication2Release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication2Release1Version1);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                        context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    Sort = DataSetsListRequestSortBy.Published,
                    SortDirection = sortDirection
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

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
            public async Task SortByRelevanceAscending_SortsByRelevanceInAscendingOrder()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0], numberOfDataSets: 3);

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(release1Version1Files[0].Id, 2),
                    new(release1Version1Files[1].Id, 3),
                    new(release1Version1Files[2].Id, 1)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var contentDbContext = ContentDbContextMock(
                    publication.ReleaseVersions,
                    release1Version1Files,
                    freeTextRanks);

                var client = BuildApp(contentDbContext.Object)
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    SearchTerm = "aaa",
                    Sort = DataSetsListRequestSortBy.Relevance,
                    SortDirection = SortDirection.Asc
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

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
                SortDirection? sortDirection)
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0], numberOfDataSets: 3);

                var freeTextRanks = new List<FreeTextRank>
                {
                    new(release1Version1Files[0].Id, 2),
                    new(release1Version1Files[1].Id, 3),
                    new(release1Version1Files[2].Id, 1)
                };

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var contentDbContext = ContentDbContextMock(
                    publication.ReleaseVersions,
                    release1Version1Files,
                    freeTextRanks);

                var client = BuildApp(contentDbContext.Object)
                    .CreateClient();

                var query = new DataSetFileListRequest
                {
                    SearchTerm = "aaa",
                    Sort = DataSetsListRequestSortBy.Relevance,
                    SortDirection = sortDirection
                };
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

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

        public class SupersededPublicationTests : ListDataSetFilesTests
        {
            public SupersededPublicationTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Fact]
            public async Task PublicationIsSuperseded_DataSetsOfSupersededPublicationsAreExcluded()
            {
                Publication supersedingPublication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                Publication supersededPublication = _fixture
                    .DefaultPublication()
                    .WithSupersededBy(supersedingPublication)
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var supersedingPublicationReleaseFiles = GenerateDataSetFilesForReleaseVersion(supersedingPublication.ReleaseVersions[0]);
                var supersededPublicationReleaseFiles = GenerateDataSetFilesForReleaseVersion(supersededPublication.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(supersedingPublicationReleaseFiles);
                        context.ReleaseFiles.AddRange(supersededPublicationReleaseFiles);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

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
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                Publication supersededPublication = _fixture
                    .DefaultPublication()
                    .WithSupersededBy(supersedingPublication)
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var supersedingPublicationReleaseFiles = GenerateDataSetFilesForReleaseVersion(supersedingPublication.ReleaseVersions[0]);
                var supersededPublicationReleaseFiles = GenerateDataSetFilesForReleaseVersion(supersededPublication.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(supersedingPublicationReleaseFiles);
                        context.ReleaseFiles.AddRange(supersededPublicationReleaseFiles);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest(PublicationId: supersededPublication.Id);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

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
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                Publication supersededPublication = _fixture
                    .DefaultPublication()
                    .WithSupersededBy(supersedingPublication)
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var supersedingPublicationReleaseFiles = GenerateDataSetFilesForReleaseVersion(supersedingPublication.ReleaseVersions[0]);
                var supersededPublicationReleaseFiles = GenerateDataSetFilesForReleaseVersion(supersededPublication.ReleaseVersions[0]);

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.AddRange(supersedingPublicationReleaseFiles);
                        context.ReleaseFiles.AddRange(supersededPublicationReleaseFiles);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                // The superseded status should be ignored as the superseding publication has no published releases
                // and all data set files belonging to the superseded publication should be returned

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: supersededPublicationReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(supersededPublicationReleaseFiles, pagedResult.Results);
            }
        }

        public class ValidationTests : ListDataSetFilesTests
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

                var query = new DataSetFileListRequest(Page: page);
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasInclusiveBetweenError("page", from: 1, to: 9999);
            }

            [Theory]
            [InlineData(1)]
            [InlineData(9999)]
            public async Task PageInAllowedRange_Success(int page)
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .CreateClient();

                var query = new DataSetFileListRequest(Page: page);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults(expectedPage: page);
            }

            [Theory]
            [InlineData(-1)]
            [InlineData(0)]
            [InlineData(41)]
            public async Task PageSizeOutsideAllowedRange_ReturnsValidationError(int pageSize)
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetFileListRequest(PageSize: pageSize);
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasInclusiveBetweenError("pageSize", from: 1, to: 40);
            }

            [Theory]
            [InlineData(1)]
            [InlineData(40)]
            public async Task PageSizeInAllowedRange_Success(int pageSize)
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .CreateClient();

                var query = new DataSetFileListRequest(PageSize: pageSize);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults(expectedPageSize: pageSize);
            }

            [Theory]
            [InlineData("a")]
            [InlineData("aa")]
            public async Task SearchTermBelowMinimumLength_ReturnsValidationError(string searchTerm)
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetFileListRequest(SearchTerm: searchTerm);
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasMinimumLengthError("searchTerm", minLength: 3);
            }

            [Theory]
            [InlineData("aaa")]
            [InlineData("aaaa")]
            public async Task SearchTermAboveMinimumLength_Success(string searchTerm)
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var contentDbContext = ContentDbContextMock();

                var client = BuildApp(contentDbContext.Object)
                    .CreateClient();

                var query = new DataSetFileListRequest(SearchTerm: searchTerm);
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();
            }

            [Fact]
            public async Task SortByNaturalWithoutReleaseId_ReturnsValidationError()
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetFileListRequest(
                    ReleaseId: null,
                    Sort: DataSetsListRequestSortBy.Natural
                );
                var response = await ListDataSets(client, query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasNotEmptyError("releaseId");
            }

            [Fact]
            public async Task SortByNaturalWithReleaseId_Success()
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp().CreateClient();

                var query = new DataSetFileListRequest(
                    ReleaseId: Guid.NewGuid(),
                    Sort: DataSetsListRequestSortBy.Natural
                );
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();
            }

            [Fact]
            public async Task SortByRelevanceWithoutSearchTerm_ReturnsValidationError()
            {
                var client = BuildApp().CreateClient();

                var query = new DataSetFileListRequest(
                    SearchTerm: null,
                    Sort: DataSetsListRequestSortBy.Relevance
                );
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasNotEmptyError("searchTerm");
            }

            [Fact]
            public async Task SortByRelevanceWithSearchTerm_Success()
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var contentDbContext = ContentDbContextMock();

                var client = BuildApp(contentDbContext.Object)
                    .CreateClient();

                var query = new DataSetFileListRequest(
                    SearchTerm: "aaa",
                    Sort: DataSetsListRequestSortBy.Relevance
                );
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();
            }
        }

        public class MiscellaneousTests : ListDataSetFilesTests
        {
            public MiscellaneousTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
            {
            }

            [Fact]
            public async Task DataSetFileMetaCorrectlyReturned_Success()
            {
                var publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()))
                    .Generate();

                var releaseFile = _fixture.DefaultReleaseFile()
                    .WithReleaseVersion(publication.ReleaseVersions[0])
                    .WithFile(_fixture.DefaultFile()
                        .WithType(FileType.Data)
                        .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta()
                            .WithGeographicLevels(["National", "Local authority"])
                            .WithTimeIdentifier(TimeIdentifier.AcademicYear)
                            .WithYears([2000, 2001, 2002])
                            .WithFilters([
                                new FilterMeta{ Label = "Filter 1" },
                                new FilterMeta{ Label = "Filter 2" },
                                new FilterMeta{ Label = "Filter 3" },
                            ])
                            .WithIndicators([
                                new IndicatorMeta { Label = "Indicator 1"},
                                new IndicatorMeta { Label = "Indicator 2"},
                                new IndicatorMeta { Label = "Indicator 3"},
                            ])
                        ))
                    .Generate();

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey,
                        PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context =>
                    {
                        context.ReleaseFiles.Add(releaseFile);
                    })
                    .CreateClient();

                var query = new DataSetFileListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var dataSetFileSummaryViewModel = Assert.Single(pagedResult.Results);
                var dataSetFileMetaViewModel = dataSetFileSummaryViewModel.Meta;

                var originalMeta = releaseFile.File.DataSetFileMeta;

                originalMeta!.GeographicLevels
                    .AssertDeepEqualTo(dataSetFileMetaViewModel.GeographicLevels);

                new DataSetFileTimePeriodViewModel
                {
                    TimeIdentifier = originalMeta.TimeIdentifier.GetEnumLabel(),
                    From = TimePeriodLabelFormatter.Format(originalMeta.Years.First(), originalMeta.TimeIdentifier),
                    To = TimePeriodLabelFormatter.Format(originalMeta.Years.Last(), originalMeta.TimeIdentifier),
                }.AssertDeepEqualTo(dataSetFileMetaViewModel.TimePeriod);

                originalMeta.Filters
                    .Select(f => f.Label)
                    .ToList()
                    .AssertDeepEqualTo(dataSetFileMetaViewModel.Filters);

                originalMeta.Indicators
                    .Select(i => i.Label)
                    .ToList()
                    .AssertDeepEqualTo(dataSetFileMetaViewModel.Indicators);
            }

            [Fact]
            public async Task NoPublishedDataSets_ReturnsEmpty()
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp().CreateClient();

                var query = new DataSetFileListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults();
            }

            [Fact]
            // TODO Remove this once we do further work to remove all HTML from summaries at source
            public async Task ReleaseFileSummariesContainHtml_HtmlTagsAreStripped()
            {
                Publication publication = _fixture
                    .DefaultPublication()
                    .WithReleases(_fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                    .WithTopic(_fixture.DefaultTopic()
                        .WithTheme(_fixture.DefaultTheme()));

                var release1Version1Files = GenerateDataSetFilesForReleaseVersion(publication.ReleaseVersions[0]);

                release1Version1Files.ForEach(releaseFile =>
                {
                    releaseFile.Summary = $"<p>{releaseFile.Summary}</p>";
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var client = BuildApp()
                    .AddContentDbTestData(context => context.ReleaseFiles.AddRange(release1Version1Files))
                    .CreateClient();

                var query = new DataSetFileListRequest();
                var response = await ListDataSets(client, query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                Assert.All(pagedResult.Results, item =>
                {
                    var content = item.Content;
                    Assert.DoesNotContain("<p>", content);
                    Assert.DoesNotContain("</p>", content);
                });
            }
        }

        private static async Task<HttpResponseMessage> ListDataSets(
            HttpClient client,
            DataSetFileListRequest request)
        {
            var queryParams = new Dictionary<string, string?>
            {
                { "themeId", request.ThemeId?.ToString() },
                { "publicationId", request.PublicationId?.ToString() },
                { "releaseId", request.ReleaseId?.ToString() },
                { "latestOnly", request.LatestOnly?.ToString() },
                { "searchTerm", request.SearchTerm },
                { "sort", request.Sort?.ToString() },
                { "sortDirection", request.SortDirection?.ToString() },
                { "page", request.Page.ToString() },
                { "pageSize", request.PageSize.ToString() },
                { "dataSetType", request.DataSetType?.ToString() }
            };

            var uri = QueryHelpers.AddQueryString("/api/data-set-files", queryParams);

            return await client.GetAsync(uri);
        }

        private static void AssertResultsForExpectedReleaseFiles(
            List<ReleaseFile> releaseFiles,
            List<DataSetFileSummaryViewModel> viewModels)
        {
            Assert.Equal(releaseFiles.Count, viewModels.Count);
            Assert.All(releaseFiles.Zip(viewModels), tuple =>
            {
                var (releaseFile, viewModel) = tuple;

                var releaseVersion = releaseFile.ReleaseVersion;
                var publication = releaseVersion.Publication;
                var theme = publication.Topic.Theme;

                Assert.Multiple(
                    () => Assert.Equal(releaseFile.FileId, viewModel.FileId),
                    () => Assert.Equal(releaseFile.File.Filename, viewModel.Filename),
                    () => Assert.Equal(releaseFile.File.DisplaySize(), viewModel.FileSize),
                    () => Assert.Equal("csv", viewModel.FileExtension),
                    () => Assert.Equal(releaseFile.Name, viewModel.Title),
                    () => Assert.Equal(releaseFile.Summary, viewModel.Content),
                    () => Assert.Equal(releaseVersion.Id, viewModel.Release.Id),
                    () => Assert.Equal(releaseVersion.Title, viewModel.Release.Title),
                    () => Assert.Equal(publication.Id, viewModel.Publication.Id),
                    () => Assert.Equal(publication.Title, viewModel.Publication.Title),
                    () => Assert.Equal(theme.Id, viewModel.Theme.Id),
                    () => Assert.Equal(theme.Title, viewModel.Theme.Title),
                    () => Assert.Equal(releaseVersion.Id == publication.LatestPublishedReleaseVersionId,
                        viewModel.LatestData),
                    () => Assert.Equal(releaseFile.ReleaseVersion.Published!.Value, viewModel.Published),
                    () => Assert.Equal(releaseFile.File.PublicDataSetVersionId.HasValue,
                        viewModel.HasApiDataSet)
                );
            });
        }

        private List<ReleaseFile> GenerateDataSetFilesForReleaseVersion(
            ReleaseVersion releaseVersion,
            int numberOfDataSets = 2)
        {
            return _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .WithFiles(_fixture.DefaultFile()
                    .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta())
                    .GenerateList(numberOfDataSets))
                .GenerateList();
        }

        private static Mock<ContentDbContext> ContentDbContextMock(
            IEnumerable<ReleaseVersion>? releaseVersions = null,
            IEnumerable<ReleaseFile>? releaseFiles = null,
            IEnumerable<FreeTextRank>? freeTextRanks = null)
        {
            var contentDbContext = new Mock<ContentDbContext>();
            contentDbContext.Setup(context => context.ReleaseVersions)
                .Returns((releaseVersions ?? Array.Empty<ReleaseVersion>()).AsQueryable().BuildMockDbSet().Object);
            contentDbContext.Setup(context => context.ReleaseFiles)
                .Returns((releaseFiles ?? Array.Empty<ReleaseFile>()).AsQueryable().BuildMockDbSet().Object);
            contentDbContext.Setup(context => context.ReleaseFilesFreeTextTable(It.IsAny<string>()))
                .Returns((freeTextRanks ?? Array.Empty<FreeTextRank>()).AsQueryable().BuildMockDbSet().Object);
            return contentDbContext;
        }
    }

    public class GetDataSetFileTests : DataSetFilesControllerTests
    {
        public GetDataSetFileTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task FetchDataSetDetails_Success()
        {
            Publication publication = _fixture.DefaultPublication()
                .WithReleases(
                    _fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()));

            ReleaseFile releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[0])
                .WithFile(_fixture.DefaultFile()
                    .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta()));

            var client = BuildApp()
                .AddContentDbTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                })
                .CreateClient();

            var uri = $"/api/data-set-files/{releaseFile.File.DataSetFileId}";

            var response = await client.GetAsync(uri);
            var viewModel = response.AssertOk<DataSetFileViewModel>();

            Assert.Equal(releaseFile.Name, viewModel.Title);
            Assert.Equal(releaseFile.Summary, viewModel.Summary);

            var file = releaseFile.File;

            Assert.Equal(file.Id, viewModel.File.Id);
            Assert.Equal(file.Filename, viewModel.File.Name);
            Assert.Equal(file.DisplaySize(), viewModel.File.Size);

            Assert.Equal(releaseFile.ReleaseVersionId, viewModel.Release.Id);
            Assert.Equal(releaseFile.ReleaseVersion.Title, viewModel.Release.Title);
            Assert.Equal(releaseFile.ReleaseVersion.Slug, viewModel.Release.Slug);
            Assert.Equal(releaseFile.ReleaseVersion.Type, viewModel.Release.Type);
            Assert.True(viewModel.Release.IsLatestPublishedRelease);
            Assert.Equal(releaseFile.ReleaseVersion.Published, viewModel.Release.Published);

            Assert.Equal(publication.Id, viewModel.Release.Publication.Id);
            Assert.Equal(publication.Title, viewModel.Release.Publication.Title);
            Assert.Equal(publication.Slug, viewModel.Release.Publication.Slug);
            Assert.Equal(publication.Topic.Theme.Title, viewModel.Release.Publication.ThemeTitle);

            var dataSetFileMeta = file.DataSetFileMeta;

            dataSetFileMeta!.GeographicLevels
                .AssertDeepEqualTo(viewModel.Meta.GeographicLevels);

            new DataSetFileTimePeriodViewModel
            {
                TimeIdentifier = dataSetFileMeta.TimeIdentifier.GetEnumLabel(),
                From =
                    TimePeriodLabelFormatter.Format(dataSetFileMeta.Years.First(), dataSetFileMeta.TimeIdentifier),
                To = TimePeriodLabelFormatter.Format(dataSetFileMeta.Years.Last(), dataSetFileMeta.TimeIdentifier),
            }.AssertDeepEqualTo(viewModel.Meta.TimePeriod);

            dataSetFileMeta.Filters
                .Select(f => f.Label)
                .ToList()
                .AssertDeepEqualTo(viewModel.Meta.Filters);

            dataSetFileMeta.Indicators
                .Select(i => i.Label)
                .ToList()
                .AssertDeepEqualTo(viewModel.Meta.Indicators);
        }

        [Fact]
        public async Task FetchDataSetFiltersOrdered_Success()
        {
            Publication publication = _fixture.DefaultPublication()
                .WithReleases(
                    _fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()));

            var filter1Id = Guid.NewGuid();
            var filter2Id = Guid.NewGuid();
            var filter3Id = Guid.NewGuid();

            ReleaseFile releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[0])
                .WithFilterSequence([
                    new FilterSequenceEntry(filter1Id, new List<FilterGroupSequenceEntry>()),
                    new FilterSequenceEntry(filter2Id, new List<FilterGroupSequenceEntry>()),
                    new FilterSequenceEntry(filter3Id, new List<FilterGroupSequenceEntry>()),
                ])
                .WithFile(_fixture.DefaultFile()
                    .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta()
                        .WithFilters([
                            new FilterMeta { Id = filter3Id, Label = "Filter 3", },
                            new FilterMeta { Id = filter1Id, Label = "Filter 1", },
                            new FilterMeta { Id = filter2Id, Label = "Filter 2", },
                        ])));

            var client = BuildApp()
                .AddContentDbTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                })
                .CreateClient();

            var uri = $"/api/data-set-files/{releaseFile.File.DataSetFileId}";

            var response = await client.GetAsync(uri);
            var viewModel = response.AssertOk<DataSetFileViewModel>();

            Assert.Equal(3, viewModel.Meta.Filters.Count);
            Assert.Equal("Filter 1", viewModel.Meta.Filters[0]);
            Assert.Equal("Filter 2", viewModel.Meta.Filters[1]);
            Assert.Equal("Filter 3", viewModel.Meta.Filters[2]);
        }

        [Fact]
        public async Task FetchDataSetIndicatorsOrdered_Success()
        {
            Publication publication = _fixture.DefaultPublication()
                .WithReleases(
                    _fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()));

            var indicator1Id = Guid.NewGuid();
            var indicator2Id = Guid.NewGuid();
            var indicator3Id = Guid.NewGuid();
            var indicator4Id = Guid.NewGuid();

            ReleaseFile releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[0])
                .WithIndicatorSequence([
                    new IndicatorGroupSequenceEntry(Guid.NewGuid(),
                        new List<Guid> { indicator1Id, }),
                    new IndicatorGroupSequenceEntry(Guid.NewGuid(),
                        new List<Guid> { indicator2Id, }),
                    new IndicatorGroupSequenceEntry(Guid.NewGuid(),
                        new List<Guid> { indicator3Id, indicator4Id })
                ])
                .WithFile(_fixture.DefaultFile()
                    .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta()
                        .WithIndicators([
                            new IndicatorMeta { Id = indicator3Id, Label = "Indicator 3", },
                            new IndicatorMeta { Id = indicator2Id, Label = "Indicator 2", },
                            new IndicatorMeta { Id = indicator1Id, Label = "Indicator 1", },
                            new IndicatorMeta { Id = indicator4Id, Label = "Indicator 4", },
                        ])));

            var client = BuildApp()
                .AddContentDbTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                })
                .CreateClient();

            var uri = $"/api/data-set-files/{releaseFile.File.DataSetFileId}";

            var response = await client.GetAsync(uri);
            var viewModel = response.AssertOk<DataSetFileViewModel>();

            Assert.Equal(4, viewModel.Meta.Indicators.Count);
            Assert.Equal("Indicator 1", viewModel.Meta.Indicators[0]);
            Assert.Equal("Indicator 2", viewModel.Meta.Indicators[1]);
            Assert.Equal("Indicator 3", viewModel.Meta.Indicators[2]);
            Assert.Equal("Indicator 4", viewModel.Meta.Indicators[3]);
        }

        [Fact]
        public async Task NoDataSetFile_ReturnsNotFound()
        {
            Publication publication = _fixture.DefaultPublication()
                .WithReleases(
                    _fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()));

            ReleaseFile releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[0])
                .WithFile(_fixture.DefaultFile());

            var client = BuildApp()
                .AddContentDbTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                })
                .CreateClient();

            var uri = $"/api/data-set-files/{Guid.NewGuid()}";

            var response = await client.GetAsync(uri);

            response.AssertNotFound();
        }

        [Fact]
        public async Task ReleaseNotPublished_ReturnsNotFound()
        {
            Publication publication = _fixture.DefaultPublication()
                .WithReleases(
                    _fixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()));

            ReleaseFile releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[0])
                .WithFile(_fixture.DefaultFile());

            var client = BuildApp()
                .AddContentDbTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                })
                .CreateClient();

            var uri = $"/api/data-set-files/{releaseFile.File.DataSetFileId}";

            var response = await client.GetAsync(uri);

            response.AssertNotFound();
        }

        [Fact]
        public async Task AmendmentNotPublished_ReturnsOk()
        {
            Publication publication = _fixture.DefaultPublication()
                .WithReleases(
                    _fixture.DefaultRelease(publishedVersions: 2, draftVersion: true)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()));

            File file = _fixture.DefaultFile()
                .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta());

            ReleaseFile releaseFile0 = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[0]) // the previous published version
                .WithFile(file);

            ReleaseFile releaseFile1 = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[1]) // the latest published version
                .WithFile(file);

            ReleaseFile releaseFile2 = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[2]) // the draft version
                .WithFile(file);

            var client = BuildApp()
                .AddContentDbTestData(context =>
                {
                    context.ReleaseFiles.AddRange(releaseFile0, releaseFile1, releaseFile2);
                })
                .CreateClient();

            var uri = $"/api/data-set-files/{releaseFile2.File.DataSetFileId}";

            var response = await client.GetAsync(uri);

            var viewModel = response.AssertOk<DataSetFileViewModel>();

            // Fetches latest published version, not amendment
            Assert.Equal(publication.ReleaseVersions[1].Id, viewModel.Release.Id);
        }

        [Fact]
        public async Task DataSetFileRemovedOnAmendment_ReturnsNotFound()
        {
            Publication publication = _fixture.DefaultPublication()
                .WithReleases(
                    _fixture.DefaultRelease(publishedVersions: 2, draftVersion: false)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()));

            File file = _fixture.DefaultFile();

            ReleaseFile releaseFile0 = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[0]) // the previous published version
                .WithFile(file);

            // NOTE: No ReleaseFile for publication.ReleaseVersions[1]

            var client = BuildApp()
                .AddContentDbTestData(context =>
                {
                    context.ReleaseFiles.Add(releaseFile0);
                })
                .CreateClient();

            var uri = $"/api/data-set-files/{releaseFile0.File.DataSetFileId}";

            var response = await client.GetAsync(uri);

            response.AssertNotFound();
        }
    }

    private WebApplicationFactory<TestStartup> BuildApp(
        ContentDbContext? contentDbContext = null)
    {
        return TestApp
            .ResetDbContexts()
            .ConfigureServices(services =>
            {
                services.AddTransient<IReleaseVersionRepository>(s => new ReleaseVersionRepository(
                    contentDbContext ?? s.GetRequiredService<ContentDbContext>()));
                services.AddTransient<IDataSetFileService>(
                    s => new DataSetFileService(
                        contentDbContext ?? s.GetRequiredService<ContentDbContext>(),
                        s.GetRequiredService<IReleaseVersionRepository>()));
            });
    }
}
