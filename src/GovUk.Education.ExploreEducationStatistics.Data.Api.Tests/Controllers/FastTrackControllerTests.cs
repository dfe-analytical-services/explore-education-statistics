#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    [Collection(BlobCacheServiceTests)]
    public class FastTrackControllerTests : BlobCacheServiceTestFixture
    {
        [Fact]
        public async Task Get()
        {
            var fastTrackId = Guid.NewGuid();

            var fastTrackViewModel = new FastTrackViewModel();

            var (controller, mocks) = BuildControllerAndMocks();

            var cacheKey = new FastTrackResultsCacheKey("publication", "release", fastTrackId);

            mocks
                .cacheKeyService
                .Setup(s => s.CreateCacheKeyForFastTrackResults(fastTrackId))
                .ReturnsAsync(cacheKey);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(FastTrackViewModel)))
                .ReturnsAsync(null);

            mocks
                .fastTrackService
                .Setup(s => s.GetFastTrackAndResults(fastTrackId))
                .ReturnsAsync(fastTrackViewModel);

            mocks.cacheService
                .Setup(s => s.SetItem<object>(cacheKey, fastTrackViewModel))
                .Returns(Task.CompletedTask);

            var result = await controller.Get(fastTrackId.ToString());

            VerifyAllMocks(mocks);

            result.AssertOkResult(fastTrackViewModel);
        }

        [Fact]
        public async Task Get_NotFound()
        {
            var fastTrackId = Guid.NewGuid();
            var cacheKey = new FastTrackResultsCacheKey("publication", "release", fastTrackId);

            var (controller, mocks) = BuildControllerAndMocks();

            mocks
                .cacheKeyService
                .Setup(s => s.CreateCacheKeyForFastTrackResults(fastTrackId))
                .ReturnsAsync(cacheKey);

            mocks.cacheService
                .Setup(s => s.GetItem(cacheKey, typeof(FastTrackViewModel)))
                .ReturnsAsync(null);

            mocks
                .fastTrackService
                .Setup(s => s.GetFastTrackAndResults(fastTrackId))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.Get(fastTrackId.ToString());
            VerifyAllMocks(mocks);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task Get_NotFoundCreatingCacheKey()
        {
            var fastTrackId = Guid.NewGuid();

            var (controller, mocks) = BuildControllerAndMocks();

            mocks
                .cacheKeyService
                .Setup(s => s.CreateCacheKeyForFastTrackResults(fastTrackId))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.Get(fastTrackId.ToString());
            VerifyAllMocks(mocks);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task Get_InvalidId()
        {
            var (controller, _) = BuildControllerAndMocks();
            var result = await controller.Get("InvalidGuid");
            result.AssertNotFoundResult();
        }

        [Fact]
        public void FastTrackViewModel_SerialiseAndDeserialise()
        {
            var original = new FastTrackViewModel
            {
                Query = new TableBuilderQueryViewModel
                {
                    Filters = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                    Indicators = new List<Guid>
                    {
                        Guid.NewGuid(),
                    },
                    Locations = new LocationQuery
                    {
                        Country = new List<string>
                        {
                            "England"
                        }
                    },
                    PublicationId = Guid.NewGuid(),
                    BoundaryLevel = 1234,
                    SubjectId = Guid.NewGuid(),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartCode = TimeIdentifier.April,
                        EndCode = TimeIdentifier.August,
                        EndYear = 2010,
                        StartYear = 2000
                    },
                    IncludeGeoJson = true
                },
                Configuration = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        Rows = new List<TableHeader>
                        {
                            new(Guid.NewGuid().ToString(), TableHeaderType.Indicator)
                        },
                        Columns = new List<TableHeader>
                        {
                            new(Guid.NewGuid().ToString(), TableHeaderType.Filter)
                        }
                    }
                },
                Created = DateTime.Now,
                Id = Guid.NewGuid(),
                FullTable = new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>
                    {
                        new()
                        {
                            Filters = new List<Guid>
                            {
                                Guid.NewGuid()
                            },
                            Location = new LocationViewModel
                            {
                                Country = new CodeNameViewModel
                                {
                                    Code = "code",
                                    Name = "name"
                                }
                            },
                            Measures = new Dictionary<Guid, string>
                            {
                                {
                                    Guid.NewGuid(), "value"
                                }
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
                },
                LatestData = true,
                ReleaseId = Guid.NewGuid(),
                ReleaseSlug = "Release Slug",
                LatestReleaseTitle = "Release Title"
            };

            var converted = DeserializeObject<FastTrackViewModel>(SerializeObject(original));
            converted.AssertDeepEqualTo(original);
        }

        private static (
            FastTrackController controller,
            (
                Mock<IFastTrackService> fastTrackService,
                Mock<ICacheKeyService> cacheKeyService,
                Mock<IBlobCacheService> cacheService
            ) mocks
            ) BuildControllerAndMocks()
        {
            var fastTrackService = new Mock<IFastTrackService>(Strict);
            var cacheKeyService = new Mock<ICacheKeyService>(Strict);
            var controller = new FastTrackController(fastTrackService.Object, cacheKeyService.Object);

            return (controller, (fastTrackService, cacheKeyService, CacheService));
        }
    }
}
