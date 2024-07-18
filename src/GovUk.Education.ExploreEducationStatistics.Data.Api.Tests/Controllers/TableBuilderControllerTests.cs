#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class TableBuilderControllerTests(TestApplicationFactory testApp)
        : IntegrationTestFixture(testApp)
    {
        private static readonly DataFixture Fixture = new();

        private static readonly ReleaseVersion ReleaseVersion = Fixture
            .DefaultReleaseVersion()
            .WithPublication(Fixture.DefaultPublication().Generate())
            .WithPublished(DateTime.UtcNow.AddDays(-1))
            .Generate();

        private static readonly DataBlockParent DataBlockParent = Fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(Fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(ReleaseVersion)
                .WithDates(published: DateTime.UtcNow.AddDays(-1))
                .WithQuery(new FullTableQuery
                {
                    SubjectId = Guid.NewGuid(),
                    LocationIds = [ Guid.NewGuid(), ],
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
                        Guid.NewGuid(),
                    },
                })
                .WithTable(new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        Rows = new List<TableHeader> { new("table header 1", TableHeaderType.Filter) }
                    }
                })
                .WithCharts(ListOf<IChart>(new LineChart
                {
                    Title = "Test chart",
                    Height = 400,
                    Width = 500,
                }))
                .Generate())
            .Generate();

        private static readonly DataBlockParent DataBlockParentWithNoPublishedVersion = Fixture
            .DefaultDataBlockParent()
            .WithLatestDraftVersion(Fixture
                .DefaultDataBlockVersion()
                .WithReleaseVersion(ReleaseVersion)
                .Generate())
            .Generate();

        private static readonly Guid PublicationId = ReleaseVersion.PublicationId;

        private static readonly Guid ReleaseVersionId = ReleaseVersion.Id;

        private static readonly Guid DataBlockId = DataBlockParent.LatestPublishedVersion!.Id;

        private static readonly Guid DataBlockParentId = DataBlockParent.Id;

        private static readonly FullTableQuery FullTableQuery =
            DataBlockParent.LatestPublishedVersion!.Query;

        private static readonly TableBuilderConfiguration TableConfiguration =
            DataBlockParent.LatestPublishedVersion!.Table;

        private readonly TableBuilderResultViewModel _tableBuilderResults = new()
        {
            SubjectMeta = new SubjectResultMetaViewModel
            {
                TimePeriodRange = new List<TimePeriodMetaViewModel>
                {
                    new(2020, AcademicYear),
                    new(2021, AcademicYear),
                }
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
                .PostAsync("/api/tablebuilder", new JsonNetContent(FullTableQuery));

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
                    (_, stream, _) => { stream.WriteText("Test csv"); }
                );

            var client = SetupApp(tableBuilderService: tableBuilderService.Object).CreateClient();

            var response = await client
                .PostAsync("/api/tablebuilder",
                    content: new JsonNetContent(FullTableQuery), // binds to FullTableQueryRequest
                    headers: new Dictionary<string, string> { { HeaderNames.Accept, ContentTypes.Csv } }
                );

            VerifyAllMocks(tableBuilderService);

            response.AssertOk("Test csv");
        }

        [Fact]
        public async Task Query_ReleaseId()
        {
            await TestApp.AddTestData<ContentDbContext>(context =>
                context.ReleaseVersions.Add(ReleaseVersion));

            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            tableBuilderService
                .Setup(
                    s => s.Query(
                        ReleaseVersionId,
                        ItIs.DeepEqualTo(FullTableQuery),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(tableBuilderService: tableBuilderService.Object)
                .CreateClient();

            var response = await client
                .PostAsync($"/api/tablebuilder/release/{ReleaseVersionId}",
                    new JsonNetContent(FullTableQuery));

            VerifyAllMocks(tableBuilderService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task Query_ReleaseId_Csv()
        {
            await TestApp.AddTestData<ContentDbContext>(context =>
                context.ReleaseVersions.Add(ReleaseVersion));

            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            tableBuilderService
                .Setup(
                    s => s.QueryToCsvStream(
                        ReleaseVersionId,
                        ItIs.DeepEqualTo(FullTableQuery),
                        It.IsAny<Stream>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, FullTableQuery, Stream, CancellationToken>(
                    (_, _, stream, _) => { stream.WriteText("Test csv"); }
                );

            var client = SetupApp(tableBuilderService: tableBuilderService.Object)
                .CreateClient();

            var response = await client
                .PostAsync($"/api/tablebuilder/release/{ReleaseVersionId}",
                    content: new JsonNetContent(FullTableQuery),
                    headers: new Dictionary<string, string> { { HeaderNames.Accept, ContentTypes.Csv } }
                );

            VerifyAllMocks(tableBuilderService);

            response.AssertOk("Test csv");
        }


        [Fact]
        public async Task QueryForTableBuilderResult()
        {
            await TestApp.AddTestData<ContentDbContext>(context =>
                context.DataBlockParents.Add(DataBlockParent));

            var cacheKey = new DataBlockTableResultCacheKey(publicationSlug: ReleaseVersion.Publication.Slug,
                releaseSlug: ReleaseVersion.Slug,
                DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>();

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseVersionId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .CreateClient();

            var response =
                await client.GetAsync(
                    $"http://localhost/api/tablebuilder/release/{ReleaseVersionId}/data-block/{DataBlockParentId}");

            VerifyAllMocks(BlobCacheService, dataBlockService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForTableBuilderResult_NotFound()
        {
            var client = SetupApp().CreateClient();

            var response =
                await client.GetAsync($"/api/tablebuilder/release/{ReleaseVersionId}/data-block/{DataBlockParentId}");

            response.AssertNotFound();
        }

        [Fact]
        public async Task QueryForTableBuilderResult_NotModified()
        {
            await TestApp.AddTestData<ContentDbContext>(context =>
                context.DataBlockParents.Add(DataBlockParent));

            var client = SetupApp()
                .CreateClient();

            var publishedDate = DataBlockParent
                .LatestPublishedVersion!
                .Published!
                .Value;

            // This date is the date when the Controller call is happening. If it's after the Published date but not too
            // far, this will be considered Not Modified still.
            var ifModifiedSinceDate = publishedDate.AddSeconds(1);

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{ReleaseVersionId}/data-block/{DataBlockParentId}",
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
            await TestApp.AddTestData<ContentDbContext>(context =>
                context.DataBlockParents.Add(DataBlockParent));

            var cacheKey = new DataBlockTableResultCacheKey(publicationSlug: ReleaseVersion.Publication.Slug,
                releaseSlug: ReleaseVersion.Slug,
                DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseVersionId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .CreateClient();

            var publishedDate = DataBlockParent
                .LatestPublishedVersion!
                .Published!
                .Value;

            // This date is the date when the Controller call is happening. If it's after the Published date but not too
            // far, this will be considered Not Modified still. So it will not be considered "Modified" by this date alone.
            // The eTag has changed however.
            var ifModifiedSinceDate = publishedDate.AddSeconds(1);

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{ReleaseVersionId}/data-block/{DataBlockParentId}",
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
            await TestApp.AddTestData<ContentDbContext>(context =>
                context.DataBlockParents.Add(DataBlockParent));

            var cacheKey = new DataBlockTableResultCacheKey(publicationSlug: ReleaseVersion.Publication.Slug,
                releaseSlug: ReleaseVersion.Slug,
                DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseVersionId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .CreateClient();

            // The latest published DataBlockVersion has been published since the caller last requested it, so we
            // consider this "Modified" by the published date alone.
            var yearBeforePublishedDate = DataBlockParent
                .LatestPublishedVersion!
                .Published!
                .Value
                .AddYears(-1);

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{ReleaseVersionId}/data-block/{DataBlockParentId}",
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
            await TestApp.AddTestData<ContentDbContext>(context =>
                context.DataBlockParents.Add(DataBlockParent));

            var latestReleaseVersion = new ReleaseVersion
            {
                Id = ReleaseVersionId,
                ReleaseName = "2020",
                TimePeriodCoverage = AcademicYear
            };

            var cacheKey = new DataBlockTableResultCacheKey(
                ReleaseVersion.Publication.Slug,
                ReleaseVersion.Slug,
                DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseVersionId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);

            releaseVersionRepository
                .Setup(s => s.GetLatestPublishedReleaseVersion(PublicationId, default))
                .ReturnsAsync(latestReleaseVersion);

            var client = SetupApp(
                    dataBlockService: dataBlockService.Object,
                    releaseVersionRepository: releaseVersionRepository.Object
                )
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{DataBlockParentId}");

            VerifyAllMocks(BlobCacheService, dataBlockService, releaseVersionRepository);

            var viewModel = response.AssertOk<FastTrackViewModel>();
            Assert.Equal(DataBlockParentId, viewModel.DataBlockParentId);
            Assert.Equal(ReleaseVersionId, viewModel.ReleaseId);
            Assert.Equal(ReleaseVersion.Slug, viewModel.ReleaseSlug);
            Assert.Equal(ReleaseVersion.Type, viewModel.ReleaseType);
            viewModel.Configuration.AssertDeepEqualTo(TableConfiguration);
            viewModel.FullTable.AssertDeepEqualTo(_tableBuilderResults);
            Assert.True(viewModel.LatestData);
            Assert.Equal("Academic year 2020/21", viewModel.LatestReleaseTitle);

            var queryViewModel = viewModel.Query;
            Assert.NotNull(queryViewModel);
            Assert.Equal(PublicationId, queryViewModel.PublicationId);
            Assert.Equal(FullTableQuery.SubjectId, viewModel.Query.SubjectId);
            Assert.Equal(FullTableQuery.TimePeriod, viewModel.Query.TimePeriod);
            Assert.Equal(FullTableQuery.Filters, viewModel.Query.Filters);
            Assert.Equal(FullTableQuery.Indicators, viewModel.Query.Indicators);
            Assert.Equal(FullTableQuery.LocationIds, viewModel.Query.LocationIds);
        }

        [Fact]
        public async Task QueryForFastTrack_DataBlockNotYetPublished()
        {
            await TestApp.AddTestData<ContentDbContext>(context =>
                context.DataBlockParents.Add(DataBlockParentWithNoPublishedVersion));

            var client = SetupApp()
                .CreateClient();

            var response =
                await client.GetAsync($"/api/tablebuilder/fast-track/{DataBlockParentWithNoPublishedVersion.Id}");

            VerifyAllMocks(BlobCacheService);

            response.AssertNotFound();
        }

        [Fact]
        public async Task QueryForFastTrack_NotLatestRelease()
        {
            await TestApp.AddTestData<ContentDbContext>(context =>
                context.DataBlockParents.Add(DataBlockParent));

            var latestReleaseVersion = new ReleaseVersion
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2021",
                TimePeriodCoverage = AcademicYear
            };

            var cacheKey = new DataBlockTableResultCacheKey(publicationSlug: ReleaseVersion.Publication.Slug,
                releaseSlug: ReleaseVersion.Slug,
                DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseVersionId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var releaseVersionRepository = new Mock<IReleaseVersionRepository>(Strict);

            releaseVersionRepository
                .Setup(s => s.GetLatestPublishedReleaseVersion(PublicationId, default))
                .ReturnsAsync(latestReleaseVersion);

            var client = SetupApp(
                    dataBlockService: dataBlockService.Object,
                    releaseVersionRepository: releaseVersionRepository.Object
                )
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{DataBlockParentId}");

            VerifyAllMocks(BlobCacheService, dataBlockService, releaseVersionRepository);

            var viewModel = response.AssertOk<FastTrackViewModel>();
            Assert.Equal(DataBlockParentId, viewModel.DataBlockParentId);
            Assert.Equal(ReleaseVersionId, viewModel.ReleaseId);
            Assert.Equal(ReleaseVersion.Slug, viewModel.ReleaseSlug);
            Assert.Equal(ReleaseVersion.Type, viewModel.ReleaseType);
            Assert.False(viewModel.LatestData);
            Assert.Equal("Academic year 2021/22", viewModel.LatestReleaseTitle);
        }

        private WebApplicationFactory<Startup> SetupApp(
            IDataBlockService? dataBlockService = null,
            IReleaseVersionRepository? releaseVersionRepository = null,
            ITableBuilderService? tableBuilderService = null)
        {
            return TestApp
                .ConfigureServices(
                    services =>
                    {
                        services.ReplaceService(BlobCacheService);

                        services.AddTransient(_ => dataBlockService ?? Mock.Of<IDataBlockService>(Strict));
                        services.AddTransient(_ =>
                            releaseVersionRepository ?? Mock.Of<IReleaseVersionRepository>(Strict));
                        services.AddTransient(_ => tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict));
                    }
                );
        }
    }
}
