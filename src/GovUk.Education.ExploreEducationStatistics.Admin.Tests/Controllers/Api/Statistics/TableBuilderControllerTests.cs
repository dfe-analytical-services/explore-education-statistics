#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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
using static Newtonsoft.Json.JsonConvert;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    [Collection(CacheServiceTests)]
    public class TableBuilderControllerTests :
        CacheServiceTestFixture, IClassFixture<TestApplicationFactory<TestStartup>>
    {
        private readonly WebApplicationFactory<TestStartup> _testApp;

        public TableBuilderControllerTests(TestApplicationFactory<TestStartup> testApp)
        {
            _testApp = testApp;
        }

        private static readonly Guid ReleaseId = Guid.NewGuid();
        private static readonly Guid SubjectId = Guid.NewGuid();
        private readonly Guid _dataBlockId = Guid.NewGuid();

        private static readonly ObservationQueryContext ObservationQueryContext = new()
        {
            SubjectId = SubjectId,
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

        private readonly ObservationQueryContext _query = new()
        {
            SubjectId = SubjectId,
        };

        [Fact]
        public async Task Query()
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);
            tableBuilderService
                .Setup(s => s.Query(
                    ReleaseId,
                    ItIs.DeepEqualTo(ObservationQueryContext),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_tableBuilderResults);

            var client = SetupApp(tableBuilderService: tableBuilderService.Object)
                .CreateClient();

            var response = await client.PostAsync(
                $"/api/data/tablebuilder/release/{ReleaseId}",
                new JsonNetContent(ObservationQueryContext));

            VerifyAllMocks(tableBuilderService);

            response.AssertOk(_tableBuilderResults);
        }

        [Fact]
        public async Task Query_Csv()
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);
            tableBuilderService
                .Setup(s => s.QueryToCsvStream(
                    ReleaseId,
                    ItIs.DeepEqualTo(ObservationQueryContext),
                    It.IsAny<Stream>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Instance)
                .Callback<Guid, ObservationQueryContext, Stream, CancellationToken>(
                    (_, _, stream, _) => { stream.WriteText("Test csv"); });

            var client = SetupApp(tableBuilderService: tableBuilderService.Object)
                .CreateClient();

            var response = await client.PostAsync(
                $"/api/data/tablebuilder/release/{ReleaseId}",
                content: new JsonNetContent(ObservationQueryContext),
                headers: new Dictionary<string, string>
                {
                    { HeaderNames.Accept, ContentTypes.Csv}
                });

            VerifyAllMocks(tableBuilderService);

            response.AssertOk("Test csv");
        }

        [Fact]
        public async Task QueryForDataBlock()
        {
            var cancellationToken = new CancellationToken();

            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = ReleaseId,
                Release = new Release
                {
                    Id = ReleaseId,
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = _dataBlockId,
                    Query = _query,
                    Charts = new List<IChart>()
                }
            };

            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, releaseContentBlock);

            mocks.cacheService
                .Setup(s => s.GetItemAsync(
                    ItIs.DeepEqualTo(new DataBlockTableResultCacheKey(releaseContentBlock)),
                    typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            mocks.tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            ReleaseId,
                            It.Is<ObservationQueryContext>(
                                q => q.SubjectId == _query.SubjectId
                            ),
                            cancellationToken
                        )
                )
                .ReturnsAsync(_tableBuilderResults);

            mocks.cacheService
                .Setup(s => s.SetItemAsync<object>(
                    ItIs.DeepEqualTo(new DataBlockTableResultCacheKey(releaseContentBlock)),
                    _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var result = await controller.QueryForDataBlock(ReleaseId, _dataBlockId, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall<ContentDbContext, ReleaseContentBlock>(mocks.persistenceHelper, null);

            var result = await controller.QueryForDataBlock(ReleaseId, _dataBlockId);
            VerifyAllMocks(mocks);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task QueryForDataBlock_NotDataBlockType()
        {
            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = ReleaseId,
                Release = new Release
                {
                    Id = ReleaseId,
                    Publication = new Publication
                    {
                        Id = Guid.NewGuid()
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new HtmlBlock
                {
                    Id = _dataBlockId,
                }
            };

            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, releaseContentBlock);

            var exception =
                await Assert.ThrowsAsync<TargetInvocationException>(() =>
                    controller.QueryForDataBlock(ReleaseId, _dataBlockId));
            Assert.IsType<ArgumentException>(exception.InnerException);
        }

        [Fact]
        public void TableBuilderResultViewModel_SerializeAndDeserialize()
        {
            var converted = DeserializeObject<TableBuilderResultViewModel>(SerializeObject(_tableBuilderResults));
            converted.AssertDeepEqualTo(_tableBuilderResults);
        }

        private static (TableBuilderController controller,
            (
            Mock<ITableBuilderService> tableBuilderService,
            Mock<IPersistenceHelper<ContentDbContext>> persistenceHelper,
            Mock<IBlobCacheService> cacheService) mocks)
            BuildControllerAndDependencies()
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);
            var persistenceHelper = MockPersistenceHelper<ContentDbContext>();
            var userService = AlwaysTrueUserService();

            var controller = new TableBuilderController(
                tableBuilderService.Object,
                persistenceHelper.Object,
                userService.Object
            );

            return (controller, (tableBuilderService, persistenceHelper, BlobCacheService));
        }

        private WebApplicationFactory<TestStartup> SetupApp(
            ITableBuilderService? tableBuilderService = null,
            IUserService? userService = null)
        {
            return _testApp
                .ResetDbContexts()
                .ConfigureServices(
                    services =>
                    {
                        services.AddTransient(_ => tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict));//
                        services.AddTransient(_ => userService ?? AlwaysTrueUserService().Object);
                    }
                );
        }
    }
}
