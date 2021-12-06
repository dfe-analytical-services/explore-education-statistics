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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    [Collection(BlobCacheServiceTests)]
    public class TableBuilderControllerTests : BlobCacheServiceTestFixture
    {
        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _dataBlockId = Guid.NewGuid();

        private readonly ObservationQueryContext _query = new()
        {
            SubjectId = Guid.NewGuid()
        };

        [Fact]
        public async Task Query()
        {
            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };
            
            var (controller, mocks) = BuildControllerAndDependencies();

            mocks.tableBuilderService
                .Setup(s => s.Query(_releaseId, _query, default))
                .ReturnsAsync(tableBuilderResults);
            
            var result = await controller.Query(_releaseId, _query);
            VerifyAllMocks(mocks);
            
            result.AssertOkResult(tableBuilderResults);
        }

        [Fact]
        public async Task Query_CancellationToken()
        {
            var cancellationToken = new CancellationToken();

            var (controller, mocks) = BuildControllerAndDependencies();

            // Assert that the passed in CancellationToken is passed down to the child calls.
            mocks.tableBuilderService
                .Setup(s => s.Query(_releaseId, _query, cancellationToken))
                .ReturnsAsync(new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>
                    {
                        new()
                    }
                });

            var result = await controller.Query(_releaseId, _query, cancellationToken);
            VerifyAllMocks(mocks);
            
            result.AssertOkResult();
        }

        [Fact]
        public async Task QueryForDataBlock()
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
                ContentBlock = new DataBlock
                {
                    Id = _dataBlockId,
                    Query = _query,
                    Charts = new List<IChart>()
                }
            };

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };

            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, releaseContentBlock);

            CacheService
                .Setup(s => s.GetItem(
                    IsMatchingDataBlockCacheKey(releaseContentBlock), 
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
                            default
                        )
                )
                .ReturnsAsync(tableBuilderResults);

            CacheService
                .Setup(s => s.SetItem<object>(
                    IsMatchingDataBlockCacheKey(releaseContentBlock),
                    tableBuilderResults))
                .Returns(Task.CompletedTask);

            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);
            VerifyAllMocks(mocks, CacheService);

            result.AssertOkResult(tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_CancellationToken()
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
            };// Assert that the passed in CancellationToken is passed down to the child calls.

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };
            
            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, releaseContentBlock);
            
            CacheService
                .Setup(s => s.GetItem(
                    IsMatchingDataBlockCacheKey(releaseContentBlock), 
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
                .ReturnsAsync(tableBuilderResults);

            CacheService
                .Setup(s => s.SetItem<object>(
                    IsMatchingDataBlockCacheKey(releaseContentBlock),
                    tableBuilderResults))
                .Returns(Task.CompletedTask);
            
            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId, cancellationToken);
            VerifyAllMocks(mocks);

            result.AssertOkResult(tableBuilderResults);
        }

        [Fact]
        public async Task QueryForDataBlock_NoGeoJson()
        {
            var subjectId = Guid.NewGuid();

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
                    Query = new ObservationQueryContext
                    {
                        SubjectId = subjectId,
                        IncludeGeoJson = false
                    },
                    Charts = new List<IChart>
                    {
                        new MapChart()
                    }
                }
            };

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };
            
            var (controller, mocks) = BuildControllerAndDependencies();

            SetupCall(mocks.persistenceHelper, releaseContentBlock);
            
            CacheService
                .Setup(s => s.GetItem(
                    IsMatchingDataBlockCacheKey(releaseContentBlock), 
                    typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);
            
            mocks.tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(
                                q =>
                                    q.SubjectId == subjectId && q.IncludeGeoJson == true
                            ),
                            default
                        )
                )
                .ReturnsAsync(tableBuilderResults);

            CacheService
                .Setup(s => s.SetItem<object>(
                    IsMatchingDataBlockCacheKey(releaseContentBlock),
                    tableBuilderResults))
                .Returns(Task.CompletedTask);
            
            var result = await controller.QueryForDataBlock(_releaseId, _dataBlockId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(tableBuilderResults);
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

            var exception = await Assert.ThrowsAsync<TargetInvocationException>(() => controller.QueryForDataBlock(_releaseId, _dataBlockId));
            Assert.IsType<ArgumentException>(exception.InnerException);
        }

        [Fact]
        public void TableBuilderResultViewModel_SerialiseAndDeserialise()
        {
            var original = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                    {
                        Filters = new List<string>
                        {
                            "filter1"
                        },
                        Location = new LocationViewModel
                        {
                            Country = new CodeNameViewModel
                            {
                                Code = "code",
                                Name = "name"
                            }
                        },
                        Measures = new Dictionary<string, string>
                        {
                            { "key", "value" }
                        },
                        GeographicLevel = GeographicLevel.Country,
                        TimePeriod = "2017/18"
                    }
                },
                SubjectMeta = new ResultSubjectMetaViewModel
                {
                    Filters = new Dictionary<string, FilterMetaViewModel>
                    {
                        {
                            "filter1", new FilterMetaViewModel
                            {
                                Hint = "A hint",
                                Legend = "A legend",
                                Name = "A name",
                                Options = new Dictionary<string, FilterItemsMetaViewModel>
                                {
                                    {
                                        "option1", new FilterItemsMetaViewModel
                                        {
                                            Label = "filter",
                                            Options = new List<LabelValue>
                                            {
                                                new("label", "value")
                                            }
                                        }
                                    }
                                },
                                TotalValue = "1234"
                            }
                        }
                    },
                    Footnotes = new List<FootnoteViewModel>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "footnote"
                        }
                    },
                    Indicators = new List<IndicatorMetaViewModel>
                    {
                        new()
                        {
                            Label = "A label",
                            Name = "A name",
                            Unit = "cm",
                            Value = "1234",
                            DecimalPlaces = 2
                        }
                    },
                    Locations = new List<ObservationalUnitMetaViewModel>
                    {
                        new()
                        {
                            Label = "A label",
                            Level = GeographicLevel.Institution,
                            Value = "1234",
                            GeoJson = true
                        }
                    },
                    BoundaryLevels = new List<BoundaryLevelViewModel>
                    {
                        new(1234, "boundary")
                    },
                    LocationsHierarchical = new Dictionary<string, List<LocationAttributeViewModel>>
                    {
                        {
                            "location", new List<LocationAttributeViewModel>
                            {
                                new()
                                {
                                    Label = "A label",
                                    Level = "Level",
                                    Options = new List<LocationAttributeViewModel>
                                    {
                                        new()
                                        {
                                            Label = "A label",
                                            Level = "Level",
                                            Options = new List<LocationAttributeViewModel>()
                                        }
                                    },
                                    Value = "A value",
                                    GeoJson = true
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

            var converted = DeserializeObject<TableBuilderResultViewModel>(SerializeObject(original));
            converted.AssertDeepEquals(original);
        }

        private static DataBlockTableResultCacheKey IsMatchingDataBlockCacheKey(ReleaseContentBlock releaseContentBlock)
        {
            return It.Is<DataBlockTableResultCacheKey>(cacheKey => cacheKey.AssertDeepEquals(new DataBlockTableResultCacheKey(releaseContentBlock)));
        }

        private (TableBuilderController controller, 
            (
                Mock<ITableBuilderService> tableBuilderService, 
                Mock<IPersistenceHelper<ContentDbContext>> persistenceHelper) mocks) 
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

            return (controller, (tableBuilderService, persistenceHelper));
        }
    }
}
