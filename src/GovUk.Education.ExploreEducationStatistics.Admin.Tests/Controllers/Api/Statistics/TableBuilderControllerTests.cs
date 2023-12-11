#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    public class TableBuilderControllerTests : CacheServiceTestFixture
    {
        private readonly DataFixture _fixture = new();

        private static readonly Guid ReleaseId = Guid.NewGuid();
        private static readonly Guid SubjectId = Guid.NewGuid();
        private static readonly Guid DataBlockId = Guid.NewGuid();

        private static readonly TableBuilderResultViewModel TableBuilderResults = new()
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

        private static readonly ObservationQueryContext Query = new()
        {
            SubjectId = SubjectId,
        };

        [Fact]
        public async Task QueryForDataBlock()
        {
            var cancellationToken = new CancellationToken();

            var dataBlockParent = _fixture
                .DefaultDataBlockParent()
                .WithLatestPublishedVersion(_fixture
                    .DefaultDataBlockVersion()
                    .WithReleaseId(ReleaseId)
                    .WithQuery(Query))
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
                                q => q.SubjectId == Query.SubjectId
                            ),
                            cancellationToken
                        )
                )
                .ReturnsAsync(TableBuilderResults);

            BlobCacheService
                .Setup(s => s.SetItemAsync<object>(
                    ItIs.DeepEqualTo(new DataBlockTableResultCacheKey(dataBlockVersion)),
                    TableBuilderResults))
                .Returns(Task.CompletedTask);

            var result = await controller.QueryForDataBlock(ReleaseId, dataBlockVersion.Id, cancellationToken);
            VerifyAllMocks(dataBlockService, tableBuilderService);

            result.AssertOkResult(TableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService
                .Setup(s => s.GetDataBlockVersionForRelease(ReleaseId, DataBlockId))
                .ReturnsAsync(new NotFoundResult());

            var controller = BuildController(dataBlockService: dataBlockService.Object);

            var result = await controller.QueryForDataBlock(ReleaseId, DataBlockId);
            VerifyAllMocks(dataBlockService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public void TableBuilderResultViewModel_SerializeAndDeserialize()
        {
            var converted = DeserializeObject<TableBuilderResultViewModel>(SerializeObject(TableBuilderResults));
            converted.AssertDeepEqualTo(TableBuilderResults);
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
    }
}