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
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
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
    public class TableBuilderControllerTests : IntegrationTest<TestStartup>
    {
        private static readonly DataFixture Fixture = new();

        private static readonly Release Release = Fixture
            .DefaultRelease()
            .WithPublication(Fixture.DefaultPublication().Generate())
            .WithPublished(DateTime.UtcNow.AddDays(-1))
            .Generate();

        private static readonly DataBlockParent DataBlockParent = Fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(Fixture
                .DefaultDataBlockVersion()
                .WithRelease(Release)
                .WithDates(published: DateTime.UtcNow.AddDays(-1))
                .WithQuery(new ObservationQueryContext
                {
                    SubjectId = Guid.NewGuid(),
                    Filters = new List<Guid>(),
                    Indicators = new List<Guid>(),
                    LocationIds = new List<Guid>(),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2021,
                        StartCode = CalendarYear,
                        EndYear = 2022,
                        EndCode = CalendarYear
                    }
                })
                .WithTable(new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        Rows = new List<TableHeader>
                        {
                            new("table header 1", TableHeaderType.Filter)
                        }
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
                .WithRelease(Release)
                .Generate())
            .Generate();

        private static readonly Guid PublicationId = Release.PublicationId;

        private static readonly Guid ReleaseId = Release.Id;

        private static readonly Guid DataBlockId = DataBlockParent.LatestPublishedVersion!.Id;

        private static readonly Guid DataBlockParentId = DataBlockParent.Id;

        private static readonly ObservationQueryContext ObservationQueryContext =
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
                new()
                {
                    TimePeriod = "2020_AY"
                },
                new()
                {
                    TimePeriod = "2021_AY"
                }
            },
        };

        public TableBuilderControllerTests(TestApplicationFactory<TestStartup> testApp) : base(testApp)
        {
        }

        [Fact]
        public async Task Query()
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            tableBuilderService
                .Setup(
                    s => s.Query(
                        ItIs.DeepEqualTo(ObservationQueryContext),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(tableBuilderService: tableBuilderService.Object).CreateClient();

            var response = await client
                .PostAsync("/api/tablebuilder", new JsonNetContent(ObservationQueryContext));

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
                        ItIs.DeepEqualTo(ObservationQueryContext),
                        It.IsAny<Stream>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<ObservationQueryContext, Stream, CancellationToken>(
                    (_, stream, _) => { stream.WriteText("Test csv"); }
                );

            var client = SetupApp(tableBuilderService: tableBuilderService.Object).CreateClient();

            var response = await client
                .PostAsync("/api/tablebuilder",
                    content: new JsonNetContent(ObservationQueryContext),
                    headers: new Dictionary<string, string>
                    {
                        { HeaderNames.Accept, ContentTypes.Csv }
                    }
                );

            VerifyAllMocks(tableBuilderService);

            response.AssertOk("Test csv");
        }

        [Fact]
        public async Task Query_ReleaseId()
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            tableBuilderService
                .Setup(
                    s => s.Query(
                        ReleaseId,
                        ItIs.DeepEqualTo(ObservationQueryContext),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(tableBuilderService: tableBuilderService.Object)
                .AddContentDbTestData(context => context.Releases.Add(Release))
                .CreateClient();

            var response = await client
                .PostAsync($"/api/tablebuilder/release/{ReleaseId}", new JsonNetContent(ObservationQueryContext));

            VerifyAllMocks(tableBuilderService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task Query_ReleaseId_Csv()
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            tableBuilderService
                .Setup(
                    s => s.QueryToCsvStream(
                        ReleaseId,
                        ItIs.DeepEqualTo(ObservationQueryContext),
                        It.IsAny<Stream>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, ObservationQueryContext, Stream, CancellationToken>(
                    (_, _, stream, _) => { stream.WriteText("Test csv"); }
                );

            var client = SetupApp(tableBuilderService: tableBuilderService.Object)
                .AddContentDbTestData(context => context.Releases.Add(Release))
                .CreateClient();

            var response = await client
                .PostAsync($"/api/tablebuilder/release/{ReleaseId}",
                    content: new JsonNetContent(ObservationQueryContext),
                    headers: new Dictionary<string, string>
                    {
                        { HeaderNames.Accept, ContentTypes.Csv }
                    }
                );

            VerifyAllMocks(tableBuilderService);

            response.AssertOk("Test csv");
        }


        [Fact]
        public async Task QueryForTableBuilderResult()
        {
            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>();

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .AddContentDbTestData(context => context.DataBlockParents.Add(DataBlockParent))
                .CreateClient();

            var response = await client.GetAsync($"http://localhost/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockParentId}");

            VerifyAllMocks(BlobCacheService, dataBlockService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForTableBuilderResult_NotFound()
        {
            var client = SetupApp().CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockParentId}");

            response.AssertNotFound();
        }

        [Fact]
        public async Task QueryForTableBuilderResult_NotModified()
        {
            var client = SetupApp()
                .AddContentDbTestData(context => context.DataBlockParents.Add(DataBlockParent))
                .CreateClient();

            var publishedDate = DataBlockParent
                .LatestPublishedVersion!
                .Published!
                .Value;

            // This date is the date when the Controller call is happening. If it's after the Published date but not too
            // far, this will be considered Not Modified still.
            var ifModifiedSinceDate = publishedDate.AddSeconds(1);

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockParentId}",
                new Dictionary<string, string>
                {
                    {
                        HeaderNames.IfModifiedSince,
                        ifModifiedSinceDate.ToUniversalTime().ToString("R")
                    },
                    {
                        HeaderNames.IfNoneMatch,
                        $"W/\"{TableBuilderController.ApiVersion}\""
                    }
                }
            );

            VerifyAllMocks(BlobCacheService);

            response.AssertNotModified();
        }

        [Fact]
        public async Task QueryForTableBuilderResult_ETagChanged()
        {
            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .AddContentDbTestData(context => context.DataBlockParents.Add(DataBlockParent))
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
                $"/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockParentId}",
                new Dictionary<string, string>
                {
                    {
                        HeaderNames.IfModifiedSince,
                        ifModifiedSinceDate.ToUniversalTime().ToString("R")
                    },
                    {
                        HeaderNames.IfNoneMatch,
                        "\"not the same etag\""
                    }
                }
            );

            VerifyAllMocks(BlobCacheService, dataBlockService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForTableBuilderResult_LastModifiedChanged()
        {
            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .AddContentDbTestData(context => context.DataBlockParents.Add(DataBlockParent))
                .CreateClient();

            // The latest published DataBlockVersion has been published since the caller last requested it, so we
            // consider this "Modified" by the published date alone.
            var yearBeforePublishedDate = DataBlockParent
                .LatestPublishedVersion!
                .Published!
                .Value
                .AddYears(-1);

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockParentId}",
                new Dictionary<string, string>
                {
                    {
                        HeaderNames.IfModifiedSince,
                        yearBeforePublishedDate.ToUniversalTime().ToString("R")
                    },
                    {
                        HeaderNames.IfNoneMatch,
                        $"W/\"{TableBuilderController.ApiVersion}\""
                    }
                }
            );

            VerifyAllMocks(BlobCacheService, dataBlockService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForFastTrack()
        {
            var latestRelease = new Release
            {
                Id = ReleaseId,
                ReleaseName = "2020",
                TimePeriodCoverage = AcademicYear
            };

            var cacheKey = new DataBlockTableResultCacheKey(
                Release.Publication.Slug,
                Release.Slug,
                DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var releaseRepository = new Mock<IReleaseRepository>(Strict);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(PublicationId))
                .ReturnsAsync(latestRelease);

            var client = SetupApp(
                    dataBlockService: dataBlockService.Object,
                    releaseRepository: releaseRepository.Object
                )
                .AddContentDbTestData(context => context.DataBlockParents.Add(DataBlockParent))
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{DataBlockParentId}");

            VerifyAllMocks(BlobCacheService, dataBlockService, releaseRepository);

            var viewModel = response.AssertOk<FastTrackViewModel>();
            Assert.Equal(DataBlockParentId, viewModel.DataBlockParentId);
            Assert.Equal(ReleaseId, viewModel.ReleaseId);
            Assert.Equal(Release.Slug, viewModel.ReleaseSlug);
            Assert.Equal(Release.Type, viewModel.ReleaseType);
            viewModel.Configuration.AssertDeepEqualTo(TableConfiguration);
            viewModel.FullTable.AssertDeepEqualTo(_tableBuilderResults);
            Assert.True(viewModel.LatestData);
            Assert.Equal("Academic year 2020/21", viewModel.LatestReleaseTitle);

            var queryViewModel = viewModel.Query;
            Assert.NotNull(queryViewModel);
            Assert.Equal(PublicationId, queryViewModel.PublicationId);
            Assert.Equal(ObservationQueryContext.SubjectId, viewModel.Query.SubjectId);
            Assert.Equal(ObservationQueryContext.TimePeriod, viewModel.Query.TimePeriod);
            Assert.Equal(ObservationQueryContext.Filters, viewModel.Query.Filters);
            Assert.Equal(ObservationQueryContext.Indicators, viewModel.Query.Indicators);
            Assert.Equal(ObservationQueryContext.LocationIds, viewModel.Query.LocationIds);
        }

        [Fact]
        public async Task QueryForFastTrack_DataBlockNotYetPublished()
        {
            var client = SetupApp()
                .AddContentDbTestData(context => context.DataBlockParents.Add(DataBlockParentWithNoPublishedVersion))
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{DataBlockParentWithNoPublishedVersion.Id}");

            VerifyAllMocks(BlobCacheService);

            response.AssertNotFound();
        }

        [Fact]
        public async Task QueryForFastTrack_NotLatestRelease()
        {
            var latestRelease = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2021",
                TimePeriodCoverage = AcademicYear
            };

            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockParentId);

            BlobCacheService
                .Setup(s => s.GetItemAsync(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var releaseRepository = new Mock<IReleaseRepository>(Strict);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(PublicationId))
                .ReturnsAsync(latestRelease);

            var client = SetupApp(
                    dataBlockService: dataBlockService.Object,
                    releaseRepository: releaseRepository.Object
                )
                .AddContentDbTestData(context => context.DataBlockParents.Add(DataBlockParent))
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{DataBlockParentId}");

            VerifyAllMocks(BlobCacheService, dataBlockService, releaseRepository);

            var viewModel = response.AssertOk<FastTrackViewModel>();
            Assert.Equal(DataBlockParentId, viewModel.DataBlockParentId);
            Assert.Equal(ReleaseId, viewModel.ReleaseId);
            Assert.Equal(Release.Slug, viewModel.ReleaseSlug);
            Assert.Equal(Release.Type, viewModel.ReleaseType);
            Assert.False(viewModel.LatestData);
            Assert.Equal("Academic year 2021/22", viewModel.LatestReleaseTitle);
        }

        private WebApplicationFactory<TestStartup> SetupApp(
            IDataBlockService? dataBlockService = null,
            IReleaseRepository? releaseRepository = null,
            ITableBuilderService? tableBuilderService = null)
        {
            return _testApp
                .ResetDbContexts()
                .ConfigureServices(
                    services =>
                    {
                        services.AddTransient(_ => dataBlockService ?? Mock.Of<IDataBlockService>(Strict));
                        services.AddTransient(_ => releaseRepository ?? Mock.Of<IReleaseRepository>(Strict));
                        services.AddTransient(_ => tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict));
                    }
                );
        }
    }
}
