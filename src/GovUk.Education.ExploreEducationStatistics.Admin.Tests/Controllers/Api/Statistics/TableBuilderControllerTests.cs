#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    [Collection(CacheServiceTests)]
    public class TableBuilderControllerTests :
        CacheServiceTestFixture, IClassFixture<TestApplicationFactory<TestStartup>>
    {
        private readonly WebApplicationFactory<TestStartup> _testApp;

        private readonly DataFixture _fixture = new();

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

            var client = SetupApp(
                    tableBuilderService: tableBuilderService.Object)
                .SetUser(AuthenticatedUser())
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
                .SetUser(AuthenticatedUser())
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

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseId(ReleaseId)
                    .WithQuery(_query))
                .Generate();

            var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

            var dataBlockService = new Mock<IDataBlockService>(Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);

            var controller = BuildController(
                dataBlockService: dataBlockService.Object,
                tableBuilderService: tableBuilderService.Object);

            BlobCacheService
                .Setup(s => s.GetItemAsync(
                    ItIs.DeepEqualTo(new DataBlockTableResultCacheKey(dataBlockVersion)),
                    typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null!);

            dataBlockService
                .Setup(s => s.GetDataBlockVersionForRelease(ReleaseId, dataBlockVersion.Id))
                .ReturnsAsync(dataBlockVersion);

            tableBuilderService
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

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(
                    ItIs.DeepEqualTo(new DataBlockTableResultCacheKey(dataBlockVersion)),
                    _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var result = await controller.QueryForDataBlock(ReleaseId, dataBlockVersion.Id, cancellationToken);
            VerifyAllMocks(dataBlockService, tableBuilderService);

            result.AssertOkResult(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockVersionForRelease(ReleaseId, _dataBlockId))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildController(dataBlockService: dataBlockService.Object);

            var result = await controller.QueryForDataBlock(ReleaseId, _dataBlockId);
            VerifyAllMocks(dataBlockService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public void TableBuilderResultViewModel_SerializeAndDeserialize()
        {
            var converted = DeserializeObject<TableBuilderResultViewModel>(SerializeObject(_tableBuilderResults));
            converted.AssertDeepEqualTo(_tableBuilderResults);
        }

        private static TableBuilderController BuildController(
            ITableBuilderService? tableBuilderService = null,
            IDataBlockService? dataBlockService = null)
        {
            return new TableBuilderController(
                tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict),
                AlwaysTrueUserService().Object,
                dataBlockService ?? Mock.Of<IDataBlockService>(Strict)
            );
        }

        private WebApplicationFactory<TestStartup> SetupApp(
            ITableBuilderService? tableBuilderService = null)
        {
            return _testApp.ConfigureServices(services =>
                services.ReplaceService(tableBuilderService ?? Mock.Of<ITableBuilderService>(Strict)));
        }
    }
}
