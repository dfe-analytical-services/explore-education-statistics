#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;
using Unit = GovUk.Education.ExploreEducationStatistics.Data.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    [Collection(CacheServiceTests)]
    public class TableBuilderControllerTests : CacheServiceTestFixture
    {
        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _dataBlockId = Guid.NewGuid();

        private readonly TableBuilderResultViewModel _tableBuilderResults = new()
        {
            Results = new List<ObservationViewModel>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Filters = new List<Guid>
                    {
                        Guid.NewGuid()
                    },
                    LocationId = Guid.NewGuid(),
                    Measures = new Dictionary<Guid, string>
                    {
                        { Guid.NewGuid(), "value" }
                    },
                    GeographicLevel = GeographicLevel.Country,
                    TimePeriod = "2017/18"
                }
            },
            SubjectMeta = new SubjectResultMetaViewModel
            {
                Filters = new Dictionary<string, FilterMetaViewModel>
                {
                    {
                        "filter1", new FilterMetaViewModel
                        {
                            Hint = "A hint",
                            Legend = "A legend",
                            Name = "A name",
                            Options = new Dictionary<string, FilterGroupMetaViewModel>
                            {
                                {
                                    "option1", new FilterGroupMetaViewModel
                                    {
                                        Label = "filter",
                                        Options = new List<FilterItemMetaViewModel>
                                        {
                                            new("label", Guid.NewGuid())
                                        }
                                    }
                                }
                            },
                            TotalValue = Guid.NewGuid()
                        }
                    }
                },
                Footnotes = new List<FootnoteViewModel>
                {
                    new(Guid.NewGuid(), "footnote")
                },
                Indicators = new List<IndicatorMetaViewModel>
                {
                    new()
                    {
                        Label = "A label",
                        Name = "A name",
                        Unit = Unit.Percent,
                        Value = Guid.NewGuid(),
                        DecimalPlaces = 2
                    }
                },
                BoundaryLevels = new List<BoundaryLevelViewModel>
                {
                    new(1234, "boundary")
                },
                Locations = new Dictionary<string, List<LocationAttributeViewModel>>
                {
                    {
                        "location", new List<LocationAttributeViewModel>
                        {
                            new()
                            {
                                Label = "A label",
                                Value = "A value",
                                Level = GeographicLevel.Region,
                                Options = new List<LocationAttributeViewModel>
                                {
                                    new()
                                    {
                                        Id = Guid.NewGuid(),
                                        Label = "A label",
                                        Value = "A value",
                                        GeoJson = true
                                    }
                                }
                            }
                        }
                    }
                },
                PublicationName = "Publication name",
                SubjectName = "Subject name",
                GeoJsonAvailable = true,
                TimePeriodRange = new List<TimePeriodMetaViewModel>
                {
                    new(1234, TimeIdentifier.April)
                    {
                        Label = "A label"
                    }
                }
            }
        };

        private readonly ObservationQueryContext _query = new()
        {
            SubjectId = Guid.NewGuid()
        };

        [Fact]
        public async Task Query()
        {
            var cancellationToken = new CancellationToken();

            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.tableBuilderService
                .Setup(s => s.Query(_releaseId, _query, cancellationToken))
                .ReturnsAsync(_tableBuilderResults);

            var result = await controller.Query(_releaseId, _query, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult();
        }

        [Fact]
        public async Task QueryForDataBlock()
        {
            var cancellationToken = new CancellationToken();

            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
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
                .Setup(s => s.GetItem(
                    ItIs.DeepEqualTo(new DataBlockTableResultCacheKey(releaseContentBlock)),
                    typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);

            mocks.tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(
                                q => q.SubjectId == _query.SubjectId
                            ),
                            cancellationToken
                        )
                )
                .ReturnsAsync(_tableBuilderResults);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(
                    ItIs.DeepEqualTo(new DataBlockTableResultCacheKey(releaseContentBlock)),
                    _tableBuilderResults))
                .Returns(Task.CompletedTask);

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(_tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_NotFound()
        {
            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall<ContentDbContext, ReleaseContentBlock>(mocks.persistenceHelper, null);

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);
            VerifyAllMocks(mocks);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task QueryForDataBlock_NotDataBlockType()
        {
            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
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
                    controller.QueryForDataBlock(_releaseId, _dataBlockId));
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
            var logger = new Mock<ILogger<TableBuilderController>>();

            var controller = new TableBuilderController(
                tableBuilderService.Object,
                persistenceHelper.Object,
                userService.Object,
                logger.Object
            );

            return (controller, (tableBuilderService, persistenceHelper, BlobCacheService));
        }
    }
}
