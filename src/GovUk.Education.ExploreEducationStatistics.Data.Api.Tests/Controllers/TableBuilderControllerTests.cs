#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    [Collection(CacheServiceTests)]
    public class TableBuilderControllerTests : CacheServiceTestFixture,
        IClassFixture<TestApplicationFactory<TestStartup>>
    {
        private static readonly ObservationQueryContext ObservationQueryContext = new()
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
        };

        private static readonly Guid PublicationId = Guid.NewGuid();

        private static readonly Guid ReleaseId = Guid.NewGuid();

        private static readonly Guid DataBlockId = Guid.NewGuid();

        private static readonly Release Release = new()
        {
            Id = ReleaseId,
            PublicationId = PublicationId,
            Publication = new Publication
            {
                Id = PublicationId,
                Slug = "publication-slug"
            },
            Published = DateTime.Parse("2020-11-11T12:00:00Z"),
            Slug = "2020-21"
        };

        private static readonly TableBuilderConfiguration TableConfiguration = new()
        {
            TableHeaders = new TableHeaders
            {
                Rows = new List<TableHeader>
                {
                    new("table header 1", TableHeaderType.Filter)
                }
            }
        };

        private static readonly ReleaseContentBlock ReleaseContentBlock = new()
        {
            ReleaseId = ReleaseId,
            Release = Release,
            ContentBlockId = DataBlockId,
            ContentBlock = new DataBlock
            {
                Id = DataBlockId,
                Query = ObservationQueryContext,
                Table = TableConfiguration
            }
        };

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

        private readonly WebApplicationFactory<TestStartup> _testApp;

        public TableBuilderControllerTests(TestApplicationFactory<TestStartup> testApp)
        {
            _testApp = testApp;
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
                        { HeaderNames.Accept, "text/csv" }
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
                        { HeaderNames.Accept, "text/csv" }
                    }
                );

            VerifyAllMocks(tableBuilderService);

            response.AssertOk("Test csv");
        }


        [Fact]
        public async Task QueryForDataBlock()
        {
            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockId);

            BlobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            BlobCacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>();

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .AddContentDbTestData(context => context.Releases.Add(Release))
                .CreateClient();

            var response = await client.GetAsync($"http://localhost/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockId}");

            VerifyAllMocks(BlobCacheService, dataBlockService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var client = SetupApp().CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockId}");

            response.AssertNotFound();
        }

        [Fact]
        public async Task QueryForDataBlock_NotModified()
        {
            var client = SetupApp()
                .AddContentDbTestData(context => context.ReleaseContentBlocks.Add(ReleaseContentBlock))
                .CreateClient();

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockId}",
                new Dictionary<string, string>
                {
                    {
                        HeaderNames.IfModifiedSince,
                        DateTime.Parse("2020-11-11T12:00:00Z").ToUniversalTime().ToString("R")
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
        public async Task QueryForDataBlock_ETagChanged()
        {
            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockId);

            BlobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            BlobCacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .AddContentDbTestData(context => context.ReleaseContentBlocks.Add(ReleaseContentBlock))
                .CreateClient();

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockId}",
                new Dictionary<string, string>
                {
                    {
                        HeaderNames.IfModifiedSince,
                        DateTime.Parse("2019-11-11T12:00:00Z").ToUniversalTime().ToString("R")
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
        public async Task QueryForDataBlock_LastModifiedChanged()
        {
            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockId);

            BlobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            BlobCacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(dataBlockService: dataBlockService.Object)
                .AddContentDbTestData(context => context.ReleaseContentBlocks.Add(ReleaseContentBlock))
                .CreateClient();

            var response = await client.GetAsync(
                $"/api/tablebuilder/release/{ReleaseId}/data-block/{DataBlockId}",
                new Dictionary<string, string>
                {
                    {
                        HeaderNames.IfModifiedSince,
                        DateTime.Parse("2019-11-11T12:00:00Z").ToUniversalTime().ToString("R")
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

            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockId);

            BlobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            BlobCacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var releaseRepository = new Mock<IReleaseRepository>(Strict);

            releaseRepository
                .Setup(s => s.IsLatestPublishedVersionOfRelease(ReleaseId))
                .ReturnsAsync(true);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(PublicationId))
                .ReturnsAsync(latestRelease);

            var client = SetupApp(
                    dataBlockService: dataBlockService.Object,
                    releaseRepository: releaseRepository.Object
                )
                .AddContentDbTestData(context => context.ReleaseContentBlocks.Add(ReleaseContentBlock))
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{DataBlockId}");

            VerifyAllMocks(BlobCacheService, dataBlockService, releaseRepository);

            var viewModel = response.AssertOk<FastTrackViewModel>();
            Assert.Equal(DataBlockId, viewModel.Id);
            Assert.Equal(ReleaseId, viewModel.ReleaseId);
            Assert.Equal("2020-21", viewModel.ReleaseSlug);
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
        public async Task QueryForFastTrack_NotForLatestPublishedVersionOfRelease()
        {
            var releaseRepository = new Mock<IReleaseRepository>(Strict);

            releaseRepository
                .Setup(s => s.IsLatestPublishedVersionOfRelease(ReleaseId))
                .ReturnsAsync(false);

            var client = SetupApp(releaseRepository: releaseRepository.Object)
                .AddContentDbTestData(context => context.ReleaseContentBlocks.Add(ReleaseContentBlock))
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{DataBlockId}");

            VerifyAllMocks(BlobCacheService, releaseRepository);

            response.AssertNotFound();;
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

            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockId);

            BlobCacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            BlobCacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var releaseRepository = new Mock<IReleaseRepository>(Strict);

            releaseRepository
                .Setup(s => s.IsLatestPublishedVersionOfRelease(ReleaseId))
                .ReturnsAsync(true);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(PublicationId))
                .ReturnsAsync(latestRelease);

            var client = SetupApp(
                    dataBlockService: dataBlockService.Object,
                    releaseRepository: releaseRepository.Object
                )
                .AddContentDbTestData(context => context.ReleaseContentBlocks.Add(ReleaseContentBlock))
                .CreateClient();

            var response = await client.GetAsync($"/api/tablebuilder/fast-track/{DataBlockId}");

            VerifyAllMocks(BlobCacheService, dataBlockService, releaseRepository);

            var viewModel = response.AssertOk<FastTrackViewModel>();
            Assert.Equal(DataBlockId, viewModel.Id);
            Assert.Equal(ReleaseId, viewModel.ReleaseId);
            Assert.Equal("2020-21", viewModel.ReleaseSlug);
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
