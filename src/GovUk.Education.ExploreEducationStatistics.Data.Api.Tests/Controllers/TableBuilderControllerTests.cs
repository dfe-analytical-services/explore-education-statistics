#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    [Collection(CacheServiceTests)]
    public class TableBuilderControllerTests : CacheServiceTestFixture
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
            Results = new List<ObservationViewModel>
            {
                new()
            }
        };

        [Fact]
        public async Task Query()
        {
            var cancellationToken = new CancellationToken();

            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.tableBuilderService
                .Setup(s => s.Query(ObservationQueryContext, cancellationToken))
                .ReturnsAsync(_tableBuilderResults);

            var result = await controller.Query(ObservationQueryContext, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
        }

        [Fact]
        public async Task Query_ReleaseId()
        {
            var cancellationToken = new CancellationToken();

            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.tableBuilderService
                .Setup(s => s.Query(ReleaseId, ObservationQueryContext, cancellationToken))
                .ReturnsAsync(_tableBuilderResults);

            var result = await controller.Query(ReleaseId, ObservationQueryContext, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock()
        {
            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockId);

            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, ReleaseId, Release);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall<ContentDbContext, Release>(mocks.persistenceHelper, ReleaseId, null);

            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task QueryForDataBlock_NotModified()
        {
            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, ReleaseId, Release);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        Headers =
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
                    }
                }
            };

            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);

            result.AssertNotModified();
        }

        [Fact]
        public async Task QueryForDataBlock_ETagChanged()
        {
            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockId);

            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, ReleaseId, Release);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        Headers =
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
                    }
                }
            };

            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_LastModifiedChanged()
        {
            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockId);

            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, ReleaseId, Release);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        Headers =
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
                    }
                }
            };

            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
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

            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, ReleaseContentBlock);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            mocks.releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(PublicationId))
                .ReturnsAsync(latestRelease);

            var result = await controller.QueryForFastTrack(DataBlockId);

            VerifyAllMocks(mocks);

            var viewModel = result.AssertOkResult();
            Assert.Equal(DataBlockId, viewModel.Id);
            Assert.Equal(ReleaseId, viewModel.ReleaseId);
            Assert.Equal("2020-21", viewModel.ReleaseSlug);
            Assert.Equal(TableConfiguration, viewModel.Configuration);
            Assert.Equal(_tableBuilderResults, viewModel.FullTable);
            Assert.True(viewModel.LatestData);
            Assert.Equal("Academic Year 2020/21", viewModel.LatestReleaseTitle);

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
        public async Task QueryForFastTrack_NotLatestRelease()
        {
            var latestRelease = new Release
            {
                Id = Guid.NewGuid(),
                ReleaseName = "2021",
                TimePeriodCoverage = AcademicYear
            };

            var cacheKey = new DataBlockTableResultCacheKey(Release.Publication.Slug, Release.Slug, DataBlockId);

            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, ReleaseContentBlock);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, _tableBuilderResults))
                .Returns(Task.CompletedTask);

            mocks.dataBlockService
                .Setup(s => s.GetDataBlockTableResult(ReleaseId, DataBlockId))
                .ReturnsAsync(_tableBuilderResults);

            mocks.releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(PublicationId))
                .ReturnsAsync(latestRelease);

            var result = await controller.QueryForFastTrack(DataBlockId);

            VerifyAllMocks(mocks);

            var viewModel = result.AssertOkResult();
            Assert.Equal(DataBlockId, viewModel.Id);
            Assert.Equal(ReleaseId, viewModel.ReleaseId);
            Assert.Equal("2020-21", viewModel.ReleaseSlug);
            Assert.False(viewModel.LatestData);
            Assert.Equal("Academic Year 2021/22", viewModel.LatestReleaseTitle);
        }

        private static (
            TableBuilderController controller,
            (Mock<IPersistenceHelper<ContentDbContext>> persistenceHelper,
            Mock<IBlobCacheService> cacheService,
            Mock<IDataBlockService> dataBlockService,
            Mock<IReleaseRepository> releaseRepository,
            Mock<ITableBuilderService> tableBuilderService) mocks) BuildControllerAndDependencies()
        {
            var persistenceHelper = MockPersistenceHelper<ContentDbContext>();
            var dataBlockService = new Mock<IDataBlockService>(Strict);
            var releaseRepository = new Mock<IReleaseRepository>(Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            var controller = new TableBuilderController(
                persistenceHelper.Object,
                dataBlockService.Object,
                releaseRepository.Object,
                tableBuilderService.Object
            )
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            return (controller,
                (persistenceHelper, BlobCacheService, dataBlockService, releaseRepository, tableBuilderService));
        }
    }
}
