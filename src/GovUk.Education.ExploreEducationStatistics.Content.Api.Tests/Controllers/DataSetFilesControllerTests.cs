#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public abstract class DataSetFilesControllerTests : IntegrationTestFixture
{
    private readonly DataFixture _fixture = new();

    private DataSetFilesControllerTests(TestApplicationFactory testApp) : base(testApp)
    {
    }

    public class ListDataSetFilesTests : DataSetFilesControllerTests
    {
        private ListDataSetFilesTests(TestApplicationFactory testApp) : base(testApp)
        {
        }

        public class FilterTests : ListDataSetFilesTests
        {
            public FilterTests(TestApplicationFactory testApp) : base(testApp)
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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                    context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest(ReleaseId: publication1.ReleaseVersions[0].Id);
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                    context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest(PublicationId: publication1.Id);
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                    context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest(ThemeId: publication1.Topic.ThemeId);
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(release1Version1Files);
                    context.ReleaseFiles.AddRange(release2Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[1].Id,
                    LatestOnly = latestOnly
                };
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(release1Version1Files);
                    context.ReleaseFiles.AddRange(release2Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[1].Id,
                    LatestOnly = true
                };
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                    context.ReleaseFiles.AddRange(publication1Release1Version2Files);
                    context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication1.ReleaseVersions[1].Id,
                    LatestOnly = latestOnly
                };
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(release1Version1Files);
                    context.ReleaseFiles.AddRange(release1Version2Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[0].Id,
                    LatestOnly = latestOnly
                };
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                    context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest(PublicationId: publication1.Id);
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                    context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();


                var query = new DataSetFileListRequest(ThemeId: publication1.Topic.ThemeId);
                var response = await ListDataSetFiles(query);

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
                var response = await ListDataSetFiles(query, client);

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
                var response = await ListDataSetFiles(query, client);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1ReleaseFiles);
                    context.ReleaseFiles.AddRange(publication2ReleaseFiles);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest(LatestOnly: latestOnly);
                var response = await ListDataSetFiles(query);

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
                
                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1ReleaseFiles);
                    context.ReleaseFiles.AddRange(publication2ReleaseFiles);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest(LatestOnly: false);
                var response = await ListDataSetFiles(query);

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
                        .ForIndex(0, s => s
                            .SetPublicApiDataSetId(Guid.NewGuid())
                            .SetPublicApiDataSetVersion(major: 1, minor: 0))
                        .ForIndex(1, s => s
                            .SetPublicApiDataSetId(Guid.NewGuid())
                            .SetPublicApiDataSetVersion(major: 2, minor: 0))
                        .GenerateList(5))
                    .GenerateList();

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(releaseVersionFiles);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    DataSetType = DataSetType.Api
                };

                var response = await ListDataSetFiles(query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var expectedReleaseFiles = releaseVersionFiles
                    .Where(rf => rf.File.PublicApiDataSetId.HasValue)
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
                        .ForIndex(0, s => s
                            .SetPublicApiDataSetId(Guid.NewGuid())
                            .SetPublicApiDataSetVersion(major: 1, minor: 1))
                        .ForIndex(1, s => s
                            .SetPublicApiDataSetId(Guid.NewGuid())
                            .SetPublicApiDataSetVersion(major: 2, minor: 0))
                        .GenerateList(5))
                    .GenerateList();

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(releaseVersionFiles);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    DataSetType = dataSetType
                };

                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                    context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest();
                var response = await ListDataSetFiles(query);

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

        public class SortByTests(TestApplicationFactory testApp) : ListDataSetFilesTests(testApp)
        {
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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    Sort = DataSetsListRequestSortBy.Title,
                    SortDirection = sortDirection
                };
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    Sort = DataSetsListRequestSortBy.Title,
                    SortDirection = SortDirection.Desc
                };
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(release1Version1Files);
                });
                
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[0].Id,
                    Sort = DataSetsListRequestSortBy.Natural,
                    SortDirection = sortDirection
                };
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(release1Version1Files);
                });
                
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    ReleaseId = publication.ReleaseVersions[0].Id,
                    Sort = DataSetsListRequestSortBy.Natural,
                    SortDirection = SortDirection.Desc
                };
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                    context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    Sort = DataSetsListRequestSortBy.Published,
                    SortDirection = SortDirection.Asc
                };
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(publication1Release1Version1Files);
                    context.ReleaseFiles.AddRange(publication2Release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest
                {
                    Sort = DataSetsListRequestSortBy.Published,
                    SortDirection = sortDirection
                };
                var response = await ListDataSetFiles(query);

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
                var response = await ListDataSetFiles(query, client);

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
                var response = await ListDataSetFiles(query, client);

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

        public class SupersededPublicationTests(TestApplicationFactory testApp)
            : ListDataSetFilesTests(testApp)
        {
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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(supersedingPublicationReleaseFiles);
                    context.ReleaseFiles.AddRange(supersededPublicationReleaseFiles);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest();
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(supersedingPublicationReleaseFiles);
                    context.ReleaseFiles.AddRange(supersededPublicationReleaseFiles);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest(PublicationId: supersededPublication.Id);
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(supersedingPublicationReleaseFiles);
                    context.ReleaseFiles.AddRange(supersededPublicationReleaseFiles);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest();
                var response = await ListDataSetFiles(query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                // The superseded status should be ignored as the superseding publication has no published releases
                // and all data set files belonging to the superseded publication should be returned

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: supersededPublicationReleaseFiles.Count);
                AssertResultsForExpectedReleaseFiles(supersededPublicationReleaseFiles, pagedResult.Results);
            }
        }

        public class ValidationTests(TestApplicationFactory testApp) : ListDataSetFilesTests(testApp)
        {
            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(10000)]
            public async Task PageOutsideAllowedRange_ReturnsValidationError(int page)
            {
                var query = new DataSetFileListRequest(Page: page);
                var response = await ListDataSetFiles(query);

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
                var query = new DataSetFileListRequest(Page: page);
                var response = await ListDataSetFiles(query);

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
                var query = new DataSetFileListRequest(PageSize: pageSize);
                var response = await ListDataSetFiles(query);

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

                var query = new DataSetFileListRequest(PageSize: pageSize);
                var response = await ListDataSetFiles(query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                pagedResult.AssertHasPagingConsistentWithEmptyResults(expectedPageSize: pageSize);
            }

            [Theory]
            [InlineData("a")]
            [InlineData("aa")]
            public async Task SearchTermBelowMinimumLength_ReturnsValidationError(string searchTerm)
            {
                var query = new DataSetFileListRequest(SearchTerm: searchTerm);
                var response = await ListDataSetFiles(query);

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
                var response = await ListDataSetFiles(query, client);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();
            }

            [Fact]
            public async Task SortByNaturalWithoutReleaseId_ReturnsValidationError()
            {
                var query = new DataSetFileListRequest(
                    ReleaseId: null,
                    Sort: DataSetsListRequestSortBy.Natural
                );
                var response = await ListDataSetFiles(query);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasNotEmptyError("releaseId");
            }

            [Fact]
            public async Task SortByNaturalWithReleaseId_Success()
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest(
                    ReleaseId: Guid.NewGuid(),
                    Sort: DataSetsListRequestSortBy.Natural
                );
                var response = await ListDataSetFiles(query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();
            }

            [Fact]
            public async Task SortByRelevanceWithoutSearchTerm_ReturnsValidationError()
            {
                var query = new DataSetFileListRequest(
                    SearchTerm: null,
                    Sort: DataSetsListRequestSortBy.Relevance
                );
                var response = await ListDataSetFiles( query);

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
                var response = await ListDataSetFiles(query, client);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();
            }
        }

        public class MiscellaneousTests(TestApplicationFactory testApp) : ListDataSetFilesTests(testApp)
        {
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
                            .WithGeographicLevels([GeographicLevel.Country, GeographicLevel.LocalAuthority])
                            .WithTimePeriodRange(
                                _fixture.DefaultTimePeriodRangeMeta()
                                    .WithStart("2000", TimeIdentifier.AcademicYear)
                                    .WithEnd("2002", TimeIdentifier.AcademicYear)
                            )
                            .WithFilters([
                                new FilterMeta { Id = Guid.NewGuid(), Label = "Filter 1", ColumnName = "filter_1", },
                                new FilterMeta { Id = Guid.NewGuid(), Label = "Filter 2", ColumnName = "filter_2", },
                                new FilterMeta { Id = Guid.NewGuid(), Label = "Filter 3", ColumnName = "filter_3", },
                            ])
                            .WithIndicators([
                                new IndicatorMeta { Id = Guid.NewGuid(), Label = "Indicator 1", ColumnName = "indicator_1", },
                                new IndicatorMeta { Id = Guid.NewGuid(), Label = "Indicator 2", ColumnName = "indicator_2", },
                                new IndicatorMeta { Id = Guid.NewGuid(), Label = "Indicator 3", ColumnName = "indicator_3", },
                            ])
                        ))
                    .Generate();

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.Add(releaseFile);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey,
                        PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest();
                var response = await ListDataSetFiles(query);

                MockUtils.VerifyAllMocks(MemoryCacheService);

                var pagedResult = response.AssertOk<PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var dataSetFileSummaryViewModel = Assert.Single(pagedResult.Results);
                var dataSetFileMetaViewModel = dataSetFileSummaryViewModel.Meta;

                var originalMeta = releaseFile.File.DataSetFileMeta;

                Assert.Equal(originalMeta!.GeographicLevels
                        .Select(gl => gl.GetEnumLabel())
                        .ToList(),
                    dataSetFileMetaViewModel.GeographicLevels);

                Assert.Equal(new DataSetFileTimePeriodRangeViewModel
                    {
                        From = TimePeriodLabelFormatter.Format(
                            originalMeta.TimePeriodRange.Start.Period,
                            originalMeta.TimePeriodRange.Start.TimeIdentifier),
                        To = TimePeriodLabelFormatter.Format(
                            originalMeta.TimePeriodRange.End.Period,
                            originalMeta.TimePeriodRange.End.TimeIdentifier),
                    },
                    dataSetFileMetaViewModel.TimePeriodRange);

                Assert.Equal(originalMeta.Filters
                        .Select(f => f.Label)
                        .ToList(),
                    dataSetFileMetaViewModel.Filters);

                Assert.Equal(originalMeta.Indicators
                        .Select(i => i.Label)
                        .ToList(),
                    dataSetFileMetaViewModel.Indicators);
            }

            [Fact]
            public async Task NoPublishedDataSets_ReturnsEmpty()
            {
                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest();
                var response = await ListDataSetFiles(query);

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

                await TestApp.AddTestData<ContentDbContext>(context =>
                {
                    context.ReleaseFiles.AddRange(release1Version1Files);
                });

                MemoryCacheService
                    .SetupNotFoundForAnyKey<ListDataSetFilesCacheKey, PaginatedListViewModel<DataSetFileSummaryViewModel>>();

                var query = new DataSetFileListRequest();
                var response = await ListDataSetFiles(query);

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

        private async Task<HttpResponseMessage> ListDataSetFiles(
            DataSetFileListRequest request,
            HttpClient? client = null)
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

            client ??= BuildApp().CreateClient();

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
                var publicApiDataSetVersion = releaseFile.File.PublicApiDataSetVersion;

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
                    () => Assert.Equal(releaseFile.File.PublicApiDataSetId, viewModel.Api?.Id),
                    () => Assert.Equal(
                        publicApiDataSetVersion is not null
                            ? $"{publicApiDataSetVersion.Major}.{publicApiDataSetVersion.Minor}"
                            : null,
                        viewModel.Api?.Version)
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

    public class GetDataSetFileTests(TestApplicationFactory testApp) : DataSetFilesControllerTests(testApp)
    {
        public override async Task InitializeAsync()
        {
            await TestApp.StartAzurite();
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
                    .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta()
                        .WithTimePeriodRange(
                        _fixture.DefaultTimePeriodRangeMeta()
                            .WithStart("2000", TimeIdentifier.CalendarYear)
                            .WithEnd("2001", TimeIdentifier.CalendarYear)
                        ))
                    .WithPublicApiDataSetId(Guid.NewGuid())
                    .WithPublicApiDataSetVersion(major: 1, minor: 0)
                );

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            await TestApp.StartAzurite();

            var testApp = BuildApp(enableAzurite: true);

            var publicBlobStorageService = testApp.Services.GetRequiredService<IPublicBlobStorageService>();

            var formFile = CreateDataCsvFormFile(""""
                                                 column_1,column_2,column_3
                                                 1,2,3
                                                 2,"3,4",5
                                                 3,"""4",5
                                                 4,,6
                                                 5,6,7
                                                 6,7,8
                                                 """");

            await publicBlobStorageService.UploadFile(
                containerName: BlobContainers.PublicReleaseFiles,
                releaseFile.PublicPath(),
                formFile);

            var response = await GetDataSetFile(releaseFile.File.DataSetFileId!.Value, testApp);
            var viewModel = response.AssertOk<DataSetFileViewModel>();

            Assert.Equal(releaseFile.Name, viewModel.Title);
            Assert.Equal(releaseFile.Summary, viewModel.Summary);

            var file = releaseFile.File;

            Assert.Equal(file.Id, viewModel.File.Id);
            Assert.Equal(file.Filename, viewModel.File.Name);
            Assert.Equal(file.DisplaySize(), viewModel.File.Size);
            Assert.Equal(file.SubjectId, viewModel.File.SubjectId);

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

            Assert.NotNull(viewModel.Api);
            Assert.Equal(file.PublicApiDataSetId, viewModel.Api.Id);
            Assert.Equal(
                $"{file.PublicApiDataSetVersion!.Major}.{file.PublicApiDataSetVersion.Minor}",
                viewModel.Api.Version);

            var dataSetFileMeta = file.DataSetFileMeta;

            Assert.Equal(dataSetFileMeta!.GeographicLevels
                    .Select(gl => gl.GetEnumLabel())
                    .ToList(),
                viewModel.File.Meta.GeographicLevels);

            Assert.Equal(new DataSetFileTimePeriodRangeViewModel
                {
                    From = TimePeriodLabelFormatter.Format(
                        "2000",
                        TimeIdentifier.CalendarYear),
                    To = TimePeriodLabelFormatter.Format(
                        "2001",
                        TimeIdentifier.CalendarYear),
                },
                viewModel.File.Meta.TimePeriodRange);

            Assert.Equal(dataSetFileMeta.Filters
                    .Select(f => f.Label)
                    .ToList(),
                viewModel.File.Meta.Filters);

            Assert.Equal(dataSetFileMeta.Indicators
                .Select(i => i.Label)
                .ToList(),
                viewModel.File.Meta.Indicators);

            Assert.Equal(3, viewModel.File.DataCsvPreview.Headers.Count);
            viewModel.File.DataCsvPreview.Headers
                .AssertDeepEqualTo(["column_1", "column_2", "column_3"]);

            Assert.Equal(5, viewModel.File.DataCsvPreview.Rows.Count);

            Assert.Equal(3, viewModel.File.DataCsvPreview.Rows[0].Count);
            viewModel.File.DataCsvPreview.Rows[0].AssertDeepEqualTo(["1", "2", "3"]);

            Assert.Equal(3, viewModel.File.DataCsvPreview.Rows[1].Count);
            viewModel.File.DataCsvPreview.Rows[1].AssertDeepEqualTo(["2", "3,4", "5"]);

            Assert.Equal(3, viewModel.File.DataCsvPreview.Rows[2].Count);
            viewModel.File.DataCsvPreview.Rows[2].AssertDeepEqualTo(["3", "\"4", "5"]);

            Assert.Equal(3, viewModel.File.DataCsvPreview.Rows[3].Count);
            viewModel.File.DataCsvPreview.Rows[3].AssertDeepEqualTo(["4", "", "6"]);

            Assert.Equal(3, viewModel.File.DataCsvPreview.Rows[4].Count);
            viewModel.File.DataCsvPreview.Rows[4].AssertDeepEqualTo(["5", "6", "7"]);
        }

        [Fact]
        public async Task FetchCsvWithSingleDataRow_Success()
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

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            await TestApp.StartAzurite();

            var testApp = BuildApp(enableAzurite: true);

            var publicBlobStorageService = testApp.Services.GetRequiredService<IPublicBlobStorageService>();

            var formFile = CreateDataCsvFormFile("""
                                                 column_1,column_2,column_3
                                                 1,2,3
                                                 """);

            await publicBlobStorageService.UploadFile(
                containerName: BlobContainers.PublicReleaseFiles,
                releaseFile.PublicPath(),
                formFile);

            var response = await GetDataSetFile(releaseFile.File.DataSetFileId!.Value, testApp);
            var viewModel = response.AssertOk<DataSetFileViewModel>();

            Assert.Equal(releaseFile.Name, viewModel.Title);
            Assert.Equal(releaseFile.Summary, viewModel.Summary);

            Assert.Equal(3, viewModel.File.DataCsvPreview.Headers.Count);
            viewModel.File.DataCsvPreview.Headers
                .AssertDeepEqualTo(["column_1", "column_2", "column_3"]);

            var row = Assert.Single(viewModel.File.DataCsvPreview.Rows);
            Assert.Equal(3, row.Count);
            row.AssertDeepEqualTo(["1", "2", "3"]);
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
                    new FilterSequenceEntry(filter1Id, []),
                    new FilterSequenceEntry(filter2Id, []),
                    new FilterSequenceEntry(filter3Id, []),
                ])
                .WithFile(_fixture.DefaultFile()
                    .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta()
                        .WithFilters([
                            new FilterMeta { Id = filter3Id, Label = "Filter 3", ColumnName = "filter_3", },
                            new FilterMeta { Id = filter1Id, Label = "Filter 1", ColumnName = "filter_1", },
                            new FilterMeta { Id = filter2Id, Label = "Filter 2", ColumnName = "filter_2", },
                        ])));

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            await TestApp.StartAzurite();

            var testApp = BuildApp(enableAzurite: true);

            var publicBlobStorageService = testApp.Services.GetRequiredService<IPublicBlobStorageService>();

            var formFile = CreateDataCsvFormFile("""
                                                 column_1
                                                 1
                                                 """);

            await publicBlobStorageService.UploadFile(
                containerName: BlobContainers.PublicReleaseFiles,
                releaseFile.PublicPath(),
                formFile);

            var response = await GetDataSetFile(releaseFile.File.DataSetFileId!.Value, testApp);
            var viewModel = response.AssertOk<DataSetFileViewModel>();

            Assert.Equal(3, viewModel.File.Meta.Filters.Count);
            Assert.Equal("Filter 1", viewModel.File.Meta.Filters[0]);
            Assert.Equal("Filter 2", viewModel.File.Meta.Filters[1]);
            Assert.Equal("Filter 3", viewModel.File.Meta.Filters[2]);
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
                    new IndicatorGroupSequenceEntry(Guid.NewGuid(), [indicator1Id]),
                    new IndicatorGroupSequenceEntry(Guid.NewGuid(), [indicator2Id]),
                    new IndicatorGroupSequenceEntry(Guid.NewGuid(), [indicator3Id, indicator4Id])
                ])
                .WithFile(_fixture.DefaultFile()
                    .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta()
                        .WithIndicators([
                            new IndicatorMeta { Id = indicator3Id, Label = "Indicator 3", ColumnName = "indicator_3", },
                            new IndicatorMeta { Id = indicator2Id, Label = "Indicator 2", ColumnName = "indicator_2", },
                            new IndicatorMeta { Id = indicator1Id, Label = "Indicator 1", ColumnName = "indicator_1", },
                            new IndicatorMeta { Id = indicator4Id, Label = "Indicator 4", ColumnName = "indicator_4", },
                        ])));
            
            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            await TestApp.StartAzurite();

            var testApp = BuildApp(enableAzurite: true);

            var publicBlobStorageService = testApp.Services.GetRequiredService<IPublicBlobStorageService>();

            var formFile = CreateDataCsvFormFile("""
                                                 column_1
                                                 1
                                                 """);

            await publicBlobStorageService.UploadFile(
                containerName: BlobContainers.PublicReleaseFiles,
                releaseFile.PublicPath(),
                formFile);

            var response = await GetDataSetFile(releaseFile.File.DataSetFileId!.Value, testApp);
            var viewModel = response.AssertOk<DataSetFileViewModel>();

            Assert.Equal(4, viewModel.File.Meta.Indicators.Count);
            Assert.Equal("Indicator 1", viewModel.File.Meta.Indicators[0]);
            Assert.Equal("Indicator 2", viewModel.File.Meta.Indicators[1]);
            Assert.Equal("Indicator 3", viewModel.File.Meta.Indicators[2]);
            Assert.Equal("Indicator 4", viewModel.File.Meta.Indicators[3]);
        }

        [Fact]
        public async Task FetchVariables_Success()
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
                    .WithDataSetFileMeta(_fixture.DefaultDataSetFileMeta()
                        .WithFilters([
                            new FilterMeta { Id = Guid.NewGuid(), Label = "Filter 1", ColumnName = "A_filter_1", Hint = "hint", },
                            new FilterMeta { Id = Guid.NewGuid(), Label = "Filter 2", ColumnName = "G_filter_2", },
                            new FilterMeta { Id = Guid.NewGuid(), Label = "Filter 3", ColumnName = "C_filter_3", Hint = "Another hint", },
                        ])
                        .WithIndicators([
                            new IndicatorMeta { Id = Guid.NewGuid(), Label = "Indicator 3", ColumnName = "B_indicator_3", },
                            new IndicatorMeta { Id = Guid.NewGuid(), Label = "Indicator 2", ColumnName = "E_indicator_2", },
                            new IndicatorMeta { Id = Guid.NewGuid(), Label = "Indicator 1", ColumnName = "D_indicator_1", },
                            new IndicatorMeta { Id = Guid.NewGuid(), Label = "Indicator 4", ColumnName = "F_indicator_4", },
                        ])));

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            await TestApp.StartAzurite();

            var testApp = BuildApp(enableAzurite: true);

            var publicBlobStorageService = testApp.Services.GetRequiredService<IPublicBlobStorageService>();

            var formFile = CreateDataCsvFormFile("""
                                                 column_1
                                                 1
                                                 """);

            await publicBlobStorageService.UploadFile(
                containerName: BlobContainers.PublicReleaseFiles,
                releaseFile.PublicPath(),
                formFile);

            var response = await GetDataSetFile(releaseFile.File.DataSetFileId!.Value, testApp);
            var viewModel = response.AssertOk<DataSetFileViewModel>();

            Assert.Equal(7, viewModel.File.Variables.Count);
            Assert.Equal("A_filter_1", viewModel.File.Variables[0].Value);
            Assert.Equal("Filter 1 - hint", viewModel.File.Variables[0].Label);
            Assert.Equal("B_indicator_3", viewModel.File.Variables[1].Value);
            Assert.Equal("Indicator 3", viewModel.File.Variables[1].Label);
            Assert.Equal("C_filter_3", viewModel.File.Variables[2].Value);
            Assert.Equal("Filter 3 - Another hint", viewModel.File.Variables[2].Label);
            Assert.Equal("D_indicator_1", viewModel.File.Variables[3].Value);
            Assert.Equal("Indicator 1", viewModel.File.Variables[3].Label);
            Assert.Equal("E_indicator_2", viewModel.File.Variables[4].Value);
            Assert.Equal("Indicator 2", viewModel.File.Variables[4].Label);
            Assert.Equal("F_indicator_4", viewModel.File.Variables[5].Value);
            Assert.Equal("Indicator 4", viewModel.File.Variables[5].Label);
            Assert.Equal("G_filter_2", viewModel.File.Variables[6].Value);
            Assert.Equal("Filter 2", viewModel.File.Variables[6].Label);
        }

        [Fact]
        public async Task FetchDataSetFootnotes_Success()
        {
            Publication publication = _fixture.DefaultPublication()
                .WithReleases(
                    _fixture.DefaultRelease(publishedVersions: 1)
                        .Generate(1))
                .WithTopic(_fixture.DefaultTopic()
                    .WithTheme(_fixture.DefaultTheme()));

            var subject = _fixture.DefaultSubject()
                .Generate();

            var releaseFile = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(publication.ReleaseVersions[0])
                .WithFile(_fixture.DefaultFile()
                    .WithSubjectId(subject.Id))
                .Generate();

            var statsReleaseVersion = new Data.Model.ReleaseVersion { Id = releaseFile.ReleaseVersionId };

            var releaseFootnote1 = _fixture.DefaultReleaseFootnote()
                .WithReleaseVersion(statsReleaseVersion)
                .WithFootnote(_fixture.DefaultFootnote()
                    .WithSubjects(new List<Subject> { subject }))
                .Generate();

            var filter = _fixture.DefaultFilter()
                .WithSubject(subject);

            var releaseFootnote2 = _fixture.DefaultReleaseFootnote()
                .WithReleaseVersion(statsReleaseVersion)
                .WithFootnote(_fixture.DefaultFootnote()
                    .WithFilters(new List<Filter> { filter }))
                .Generate();

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });
            await TestApp.AddTestData<StatisticsDbContext>(context =>
            {
                context.ReleaseFootnote.AddRange(releaseFootnote1, releaseFootnote2);
            });

            await TestApp.StartAzurite();

            var testApp = BuildApp(enableAzurite: true);

            var publicBlobStorageService = testApp.Services.GetRequiredService<IPublicBlobStorageService>();

            var formFile = CreateDataCsvFormFile("""
                                                 column_1
                                                 1
                                                 """);

            await publicBlobStorageService.UploadFile(
                containerName: BlobContainers.PublicReleaseFiles,
                releaseFile.PublicPath(),
                formFile);

            var response = await GetDataSetFile(releaseFile.File.DataSetFileId!.Value, testApp);
            var viewModel = response.AssertOk<DataSetFileViewModel>();

            var footnotes = viewModel.Footnotes;
            Assert.Equal(2, footnotes.Count);

            Assert.Equal("Footnote 0 :: Content", footnotes[0].Label);
            Assert.Equal("Footnote 1 :: Content", footnotes[1].Label);
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

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            var response = await GetDataSetFile(Guid.NewGuid());

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

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile);
            });

            var response = await GetDataSetFile(releaseFile.File.DataSetFileId!.Value);

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

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.AddRange(releaseFile0, releaseFile1, releaseFile2);
            });

            await TestApp.StartAzurite();

            var testApp = BuildApp(enableAzurite: true);

            var publicBlobStorageService = testApp.Services.GetRequiredService<IPublicBlobStorageService>();

            var formFile = CreateDataCsvFormFile("""
                                                 column_1
                                                 1
                                                 """);

            await publicBlobStorageService.UploadFile(
                containerName: BlobContainers.PublicReleaseFiles,
                releaseFile1.PublicPath(),
                formFile);

            var response = await GetDataSetFile(releaseFile2.File.DataSetFileId!.Value, testApp);
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

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.ReleaseFiles.Add(releaseFile0);
            });

            var response = await GetDataSetFile(releaseFile0.File.DataSetFileId!.Value);

            response.AssertNotFound();
        }

        private async Task<HttpResponseMessage> GetDataSetFile(
            Guid dataSetFileId,
            WebApplicationFactory<Startup>? app = null)
        {
            var client = (app ?? BuildApp()).CreateClient();

            return await client.GetAsync($"/api/data-set-files/{dataSetFileId}");
        }
    }

    private WebApplicationFactory<Startup> BuildApp(
        ContentDbContext? contentDbContext = null,
        StatisticsDbContext? statisticsDbContext = null,
        bool enableAzurite = false)
    {
        return TestApp
            .WithAzurite(enabled: enableAzurite)
            .ConfigureServices(services =>
            {
                services.ReplaceService(MemoryCacheService);

                if (contentDbContext is not null)
                {
                    services.ReplaceService(contentDbContext);
                }

                if (statisticsDbContext is not null)
                {
                    services.ReplaceService(statisticsDbContext);
                }
            });
    }

    private static IFormFile CreateDataCsvFormFile(string content)
    {
        var memoryStream = new MemoryStream();
        var writer = new StreamWriter(memoryStream);
        writer.Write(content);
        writer.Flush();
        memoryStream.Position = 0;

        var headerDictionary = new HeaderDictionary
        {
            ["ContentType"] = "text/csv"
        };

        return new FormFile(
            memoryStream,
            0,
            memoryStream.Length,
            "id_from_form",
            "dataCsv.csv")
        {
            Headers = headerDictionary,
        };
    }
}
