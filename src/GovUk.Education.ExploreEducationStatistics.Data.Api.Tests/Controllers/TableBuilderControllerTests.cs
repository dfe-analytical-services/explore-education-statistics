#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Utils.FilterHierarchiesOptionsUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class TableBuilderControllerTests(TestApplicationFactory testApp)
        : IntegrationTestFixture(testApp)
    {
        private readonly DataFixture _dataFixture = new();

        private static readonly List<IChart> Charts =
        [
            new LineChart
            {
                Title = "Test chart",
                Height = 400,
                Width = 500
            }
        ];

        private static readonly FullTableQuery FullTableQuery = new()
        {
            SubjectId = Guid.NewGuid(),
            LocationIds = [Guid.NewGuid()],
            TimePeriod = new TimePeriodQuery
            {
                StartYear = 2021,
                StartCode = CalendarYear,
                EndYear = 2022,
                EndCode = CalendarYear
            },
            Filters = new List<Guid>(),
            Indicators = new List<Guid> // use collection expression -> test failures
            {
                Guid.NewGuid()
            },
            FilterHierarchiesOptions = new List<FilterHierarchyOptions>
            {
                new() { LeafFilterId = Guid.NewGuid(), Options = [[Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]] },
                new() { LeafFilterId = Guid.NewGuid(), Options = [[Guid.NewGuid(), Guid.NewGuid()]] },
                new() { LeafFilterId = Guid.NewGuid(), Options = [[Guid.NewGuid(), Guid.NewGuid()]] },
                new() { LeafFilterId = Guid.NewGuid(), Options = [[Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid(), Guid.NewGuid()]] },
            }
        };

        private static readonly TableBuilderConfiguration TableConfiguration = new()
        {
            TableHeaders = new TableHeaders { Rows = [new TableHeader("table header 1", TableHeaderType.Filter)] }
        };

        private readonly TableBuilderResultViewModel _tableBuilderResults = new()
        {
            SubjectMeta = new SubjectResultMetaViewModel
            {
                TimePeriodRange =
                [
                    new TimePeriodMetaViewModel(2020, AcademicYear),
                    new TimePeriodMetaViewModel(2021, AcademicYear)
                ]
            },
            Results = new List<ObservationViewModel>
            {
                new() { TimePeriod = "2020_AY" },
                new() { TimePeriod = "2021_AY" }
            },
        };

        [Fact]
        public async Task Query()
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            tableBuilderService
                .Setup(
                    s => s.Query(
                        ItIs.DeepEqualTo(FullTableQuery),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(tableBuilderService: tableBuilderService.Object).CreateClient();

            var response = await client
                .PostAsync("/api/tablebuilder", new JsonNetContent(ToRequest(FullTableQuery)));

            VerifyAllMocks(tableBuilderService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task Query_Csv()
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            tableBuilderService
                .Setup(
                    s => s.QueryToCsvStream(
                        ItIs.DeepEqualTo(FullTableQuery),
                        It.IsAny<Stream>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<FullTableQuery, Stream, CancellationToken>(
                    (_, stream, _) => stream.WriteText("Test csv"));

            var client = SetupApp(tableBuilderService: tableBuilderService.Object).CreateClient();

            var response = await client
                .PostAsync("/api/tablebuilder",
                    content: new JsonNetContent(ToRequest(FullTableQuery)),
                    headers: new Dictionary<string, string> { { HeaderNames.Accept, ContentTypes.Csv } }
                );

            VerifyAllMocks(tableBuilderService);

            response.AssertOk("Test csv");
        }

        [Fact]
        public async Task Query_ReleaseVersionId()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var releaseVersion = publication.Releases.Single().Versions.Single();

            await TestApp.AddTestData<ContentDbContext>(context =>
                context.Publications.Add(publication));

            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            tableBuilderService
                .Setup(
                    s => s.Query(
                        releaseVersion.Id,
                        ItIs.DeepEqualTo(FullTableQuery),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(tableBuilderService: tableBuilderService.Object)
                .CreateClient();

            var response = await client
                .PostAsync($"/api/tablebuilder/release/{releaseVersion.Id}",
                    new JsonNetContent(ToRequest(FullTableQuery)));

            VerifyAllMocks(tableBuilderService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task Query_ReleaseVersionId_Csv()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            await TestApp.AddTestData<ContentDbContext>(context =>
                context.Publications.Add(publication));

            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            tableBuilderService
                .Setup(
                    s => s.QueryToCsvStream(
                        releaseVersion.Id,
                        ItIs.DeepEqualTo(FullTableQuery),
                        It.IsAny<Stream>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, FullTableQuery, Stream, CancellationToken>(
                    (_, _, stream, _) => stream.WriteText("Test csv"));

            var client = SetupApp(tableBuilderService: tableBuilderService.Object)
                .CreateClient();

            var response = await client
                .PostAsync($"/api/tablebuilder/release/{releaseVersion.Id}",
                    content: new JsonNetContent(ToRequest(FullTableQuery)),
                    headers: new Dictionary<string, string> { { HeaderNames.Accept, ContentTypes.Csv } }
                );

            VerifyAllMocks(tableBuilderService);

            response.AssertOk("Test csv");
        }


        [Fact]
        public async Task QueryForTableBuilderResult()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            DataBlockParent dataBlockParent = _dataFixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_dataFixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithDates(published: DateTime.UtcNow.AddDays(-1))
                    .WithQuery(FullTableQuery)
                    .WithTable(TableConfiguration)
                    .WithCharts(Charts));

            var dataBlockId = dataBlockParent.LatestPublishedVersion!.Id;

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.DataBlockParents.Add(dataBlockParent);
            });

            var cacheKey = new DataBlockTableResultCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: release.Slug,
                dataBlockParent.Id);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>();

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseVersion.Id, dataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .CreateClient();

            var response =
                await client.GetAsync(
                    $"http://localhost/api/tablebuilder/release/{releaseVersion.Id}/data-block/{dataBlockParent.Id}");

            VerifyAllMocks(BlobCacheService, dataBlockService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForTableBuilderResult_NotFound()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            await TestApp.AddTestData<ContentDbContext>(context => context.Publications.Add(publication));

            var client = SetupApp().CreateClient();

            var response =
                await client.GetAsync($"/api/tablebuilder/release/{releaseVersion.Id}/data-block/{Guid.NewGuid()}");

            response.AssertNotFound();
        }

        [Fact]
        public async Task QueryForTableBuilderResult_NotModified()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            DataBlockParent dataBlockParent = _dataFixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_dataFixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithDates(published: DateTime.UtcNow.AddDays(-1))
                    .WithQuery(FullTableQuery)
                    .WithTable(TableConfiguration)
                    .WithCharts(Charts));

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.DataBlockParents.Add(dataBlockParent);
            });

            var client = SetupApp()
                .CreateClient();

            var publishedDate = dataBlockParent
                .LatestPublishedVersion!
                .Published!
                .Value;

            // This date is the date when the Controller call is happening. If it's after the Published date but not too
            // far, this will be considered Not Modified still.
            var ifModifiedSinceDate = publishedDate.AddSeconds(1);

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{releaseVersion.Id}/data-block/{dataBlockParent.Id}",
                new Dictionary<string, string>
                {
                    { HeaderNames.IfModifiedSince, ifModifiedSinceDate.ToUniversalTime().ToString("R") },
                    { HeaderNames.IfNoneMatch, $"W/\"{TableBuilderController.ApiVersion}\"" }
                }
            );

            VerifyAllMocks(BlobCacheService);

            response.AssertNotModified();
        }

        [Fact]
        public async Task QueryForTableBuilderResult_ETagChanged()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            DataBlockParent dataBlockParent = _dataFixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_dataFixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithDates(published: DateTime.UtcNow.AddDays(-1))
                    .WithQuery(FullTableQuery)
                    .WithTable(TableConfiguration)
                    .WithCharts(Charts));

            var dataBlockId = dataBlockParent.LatestPublishedVersion!.Id;

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.DataBlockParents.Add(dataBlockParent);
            });

            var cacheKey = new DataBlockTableResultCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: release.Slug,
                dataBlockParent.Id);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseVersion.Id, dataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .CreateClient();

            var publishedDate = dataBlockParent
                .LatestPublishedVersion!
                .Published!
                .Value;

            // This date is the date when the Controller call is happening. If it's after the Published date but not too
            // far, this will be considered Not Modified still. So it will not be considered "Modified" by this date alone.
            // The eTag has changed however.
            var ifModifiedSinceDate = publishedDate.AddSeconds(1);

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{releaseVersion.Id}/data-block/{dataBlockParent.Id}",
                new Dictionary<string, string>
                {
                    { HeaderNames.IfModifiedSince, ifModifiedSinceDate.ToUniversalTime().ToString("R") },
                    { HeaderNames.IfNoneMatch, "\"not the same etag\"" }
                }
            );

            VerifyAllMocks(BlobCacheService, dataBlockService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForTableBuilderResult_LastModifiedChanged()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            DataBlockParent dataBlockParent = _dataFixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_dataFixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithDates(published: DateTime.UtcNow.AddDays(-1))
                    .WithQuery(FullTableQuery)
                    .WithTable(TableConfiguration)
                    .WithCharts(Charts));

            var dataBlockId = dataBlockParent.LatestPublishedVersion!.Id;

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.DataBlockParents.Add(dataBlockParent);
            });

            var cacheKey = new DataBlockTableResultCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: release.Slug,
                dataBlockParent.Id);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseVersion.Id, dataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .CreateClient();

            // The latest published DataBlockVersion has been published since the caller last requested it, so we
            // consider this "Modified" by the published date alone.
            var yearBeforePublishedDate = dataBlockParent
                .LatestPublishedVersion!
                .Published!
                .Value
                .AddYears(-1);

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{releaseVersion.Id}/data-block/{dataBlockParent.Id}",
                new Dictionary<string, string>
                {
                    { HeaderNames.IfModifiedSince, yearBeforePublishedDate.ToUniversalTime().ToString("R") },
                    { HeaderNames.IfNoneMatch, $"W/\"{TableBuilderController.ApiVersion}\"" }
                }
            );

            VerifyAllMocks(BlobCacheService, dataBlockService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForFastTrack()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([
                    _dataFixture
                        .DefaultRelease(publishedVersions: 1, year: 2020),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 1, year: 2021)
                ]);

            var release = publication.Releases.Single(r => r.Year == 2021);
            var releaseVersion = release.Versions.Single();

            DataBlockParent dataBlockParent = _dataFixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_dataFixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithDates(published: DateTime.UtcNow.AddDays(-1))
                    .WithQuery(FullTableQuery)
                    .WithTable(TableConfiguration)
                    .WithCharts(Charts));

            var dataBlockId = dataBlockParent.LatestPublishedVersion!.Id;

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.DataBlockParents.Add(dataBlockParent);
            });

            var cacheKey = new DataBlockTableResultCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: release.Slug,
                dataBlockParent.Id);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseVersion.Id, dataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(
                    dataBlockService: dataBlockService.Object
                )
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{dataBlockParent.Id}");

            VerifyAllMocks(
                BlobCacheService,
                dataBlockService
            );

            var viewModel = response.AssertOk<FastTrackViewModel>();
            Assert.Equal(dataBlockParent.Id, viewModel.DataBlockParentId);
            Assert.Equal(releaseVersion.Id, viewModel.ReleaseId);
            Assert.Equal(release.Slug, viewModel.ReleaseSlug);
            Assert.Equal(releaseVersion.Type, viewModel.ReleaseType);
            viewModel.Configuration.AssertDeepEqualTo(TableConfiguration);
            viewModel.FullTable.AssertDeepEqualTo(_tableBuilderResults);
            Assert.True(viewModel.LatestData);
            Assert.Equal(release.Title, viewModel.LatestReleaseTitle);
            Assert.Equal(release.Slug, viewModel.LatestReleaseSlug);

            var queryViewModel = viewModel.Query;
            Assert.NotNull(queryViewModel);
            Assert.Equal(publication.Id, queryViewModel.PublicationId);
            Assert.Equal(FullTableQuery.SubjectId, viewModel.Query.SubjectId);
            Assert.Equal(FullTableQuery.TimePeriod, viewModel.Query.TimePeriod);
            Assert.Equal(FullTableQuery.GetNonHierarchicalFilterItemIds(), viewModel.Query.GetNonHierarchicalFilterItemIds());
            viewModel.Query.FilterHierarchiesOptions.AssertDeepEqualTo(FullTableQuery.FilterHierarchiesOptions);
            Assert.Equal(FullTableQuery.Indicators, viewModel.Query.Indicators);
            Assert.Equal(FullTableQuery.LocationIds, viewModel.Query.LocationIds);
        }

        [Fact]
        public async Task QueryForFastTrack_DataBlockNotYetPublished()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]);

            var release = publication.Releases.Single();
            var releaseVersion = release.Versions.Single();

            DataBlockParent dataBlockParentWithNoPublishedVersion = _dataFixture
                .DefaultDataBlockParent()
                .WithLatestDraftVersion(_dataFixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion));

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.DataBlockParents.Add(dataBlockParentWithNoPublishedVersion);
            });

            var client = SetupApp()
                .CreateClient();

            var response =
                await client.GetAsync($"/api/tablebuilder/fast-track/{dataBlockParentWithNoPublishedVersion.Id}");

            VerifyAllMocks(BlobCacheService);

            response.AssertNotFound();
        }

        [Fact]
        public async Task QueryForFastTrack_NotLatestRelease()
        {
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([
                    _dataFixture
                        .DefaultRelease(publishedVersions: 1, year: 2020),
                    _dataFixture
                        .DefaultRelease(publishedVersions: 1, year: 2021)
                ]);

            var release2020 = publication.Releases.Single(r => r.Year == 2020);
            var release2021 = publication.Releases.Single(r => r.Year == 2021);

            // Release version is from the 2020 release which is not the latest release for the publication
            var releaseVersion = release2020.Versions.Single();

            DataBlockParent dataBlockParent = _dataFixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_dataFixture
                    .DefaultDataBlockVersion()
                    .WithReleaseVersion(releaseVersion)
                    .WithDates(published: DateTime.UtcNow.AddDays(-1))
                    .WithQuery(FullTableQuery)
                    .WithTable(TableConfiguration)
                    .WithCharts(Charts));

            var dataBlockId = dataBlockParent.LatestPublishedVersion!.Id;

            await TestApp.AddTestData<ContentDbContext>(context =>
            {
                context.Publications.Add(publication);
                context.DataBlockParents.Add(dataBlockParent);
            });

            var cacheKey = new DataBlockTableResultCacheKey(
                publicationSlug: publication.Slug,
                releaseSlug: release2020.Slug,
                dataBlockParent.Id);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(releaseVersion.Id, dataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(
                    dataBlockService: dataBlockService.Object
                )
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{dataBlockParent.Id}");

            VerifyAllMocks(
                BlobCacheService,
                dataBlockService
            );

            var viewModel = response.AssertOk<FastTrackViewModel>();
            Assert.Equal(dataBlockParent.Id, viewModel.DataBlockParentId);
            Assert.Equal(releaseVersion.Id, viewModel.ReleaseId);
            Assert.Equal(release2020.Slug, viewModel.ReleaseSlug);
            Assert.Equal(releaseVersion.Type, viewModel.ReleaseType);
            Assert.False(viewModel.LatestData);
            Assert.Equal(release2021.Title, viewModel.LatestReleaseTitle);
            Assert.Equal(release2021.Slug, viewModel.LatestReleaseSlug);
        }

        private WebApplicationFactory<Startup> SetupApp(
            IDataBlockService? dataBlockService = null,
            ITableBuilderService? tableBuilderService = null)
        {
            return TestApp
                .ConfigureServices(
                    services =>
                    {
                        services.ReplaceService(BlobCacheService);

                        services.AddTransient(_ => dataBlockService ?? Mock.Of<IDataBlockService>(Strict));
                        services.AddTransient(_ => tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict));
                    }
                );
        }

        private FullTableQueryRequest ToRequest(FullTableQuery query)
        {
            return new FullTableQueryRequest
            {
                SubjectId = query.SubjectId,
                LocationIds = query.LocationIds,
                TimePeriod = query.TimePeriod,
                Filters = query.GetNonHierarchicalFilterItemIds(),
                Indicators = query.Indicators,
                FilterHierarchiesOptions =
                    FilterHierarchiesOptionsAsDictionary(query.FilterHierarchiesOptions),
            };
        }
    }
}
