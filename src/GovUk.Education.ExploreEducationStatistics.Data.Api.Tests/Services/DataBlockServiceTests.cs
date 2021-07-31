using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class DataBlockServiceTests
    {
        private readonly Guid _releaseId = Guid.NewGuid();
        private readonly Guid _dataBlockId = Guid.NewGuid();

        [Fact]
        public async Task GetDataBlockTableResult()
        {
            var subjectId = Guid.NewGuid();

            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
                    Slug = "release-slug",
                    Publication = new Publication
                    {
                        Slug = "publication-slug"
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = _dataBlockId,
                    Query = new ObservationQueryContext
                    {
                        SubjectId = subjectId,
                    },
                    Charts = new List<IChart>
                    {
                        new LineChart()
                    }
                }
            };

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            await contentDbContext.AddAsync(releaseContentBlock);
            await contentDbContext.SaveChangesAsync();

            var tableResult = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new ObservationViewModel()
                }
            };

            var cacheService = new Mock<IBlobCacheService>(MockBehavior.Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(MockBehavior.Strict);

            cacheService
                .SetupGetItemForCacheMiss<TableBuilderResultViewModel,
                    Task<Either<ActionResult, TableBuilderResultViewModel>>>(
                    new DataBlockTableResultCacheKey(releaseContentBlock));

            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(q => q.SubjectId == subjectId)
                        )
                )
                .ReturnsAsync(tableResult);

            var service = BuildDataBlockService(
                contentDbContext,
                blobCacheService: cacheService.Object,
                tableBuilderService: tableBuilderService.Object
            );

            var result = (await service.GetDataBlockTableResult(releaseContentBlock)).AssertRight();

            Assert.Single(result.Results);

            VerifyAllMocks(cacheService, tableBuilderService);
        }

        [Fact]
        public async Task GetDataBlockTableResult_MapChartForcesIncludeGeoJson()
        {
            var subjectId = Guid.NewGuid();

            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
                    Slug = "release-slug",
                    Publication = new Publication
                    {
                        Slug = "publication-slug"
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = _dataBlockId,
                    Query = new ObservationQueryContext
                    {
                        SubjectId = subjectId,
                        // This is set to false in the persisted query but
                        // we expect it to be converted to true in the
                        // query that is actually ran by TableBuilderService
                        IncludeGeoJson = false
                    },
                    Charts = new List<IChart>
                    {
                        new MapChart()
                    }
                }
            };

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            await contentDbContext.AddAsync(releaseContentBlock);
            await contentDbContext.SaveChangesAsync();

            var tableResult = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new ObservationViewModel()
                }
            };

            var cacheService = new Mock<IBlobCacheService>(MockBehavior.Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(MockBehavior.Strict);

            cacheService
                .SetupGetItemForCacheMiss<TableBuilderResultViewModel,
                    Task<Either<ActionResult, TableBuilderResultViewModel>>>(
                    new DataBlockTableResultCacheKey(releaseContentBlock));

            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(
                                q =>
                                    q.SubjectId == subjectId && q.IncludeGeoJson == true
                            )
                        )
                )
                .ReturnsAsync(tableResult);

            var service = BuildDataBlockService(
                contentDbContext,
                blobCacheService: cacheService.Object,
                tableBuilderService: tableBuilderService.Object
            );

            var result = (await service.GetDataBlockTableResult(releaseContentBlock)).AssertRight();

            Assert.Single(result.Results);

            VerifyAllMocks(cacheService, tableBuilderService);
        }

        [Fact]
        public async Task GetDataBlockTableResult_CachedResultExists()
        {
            var subjectId = Guid.NewGuid();

            var releaseContentBlock = new ReleaseContentBlock
            {
                ReleaseId = _releaseId,
                Release = new Release
                {
                    Id = _releaseId,
                    Slug = "release-slug",
                    Publication = new Publication
                    {
                        Slug = "publication-slug"
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new DataBlock
                {
                    Id = _dataBlockId,
                    Query = new ObservationQueryContext
                    {
                        SubjectId = subjectId,
                    },
                    Charts = new List<IChart>
                    {
                        new LineChart()
                    }
                }
            };

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            await contentDbContext.AddAsync(releaseContentBlock);
            await contentDbContext.SaveChangesAsync();

            var cacheKey = $"publications/publication-slug/releases/release-slug/data-blocks/{_dataBlockId}.json";

            var tableResult = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new ObservationViewModel()
                }
            };

            var cacheService = new Mock<IBlobCacheService>(MockBehavior.Strict);
            var tableBuilderService = new Mock<ITableBuilderService>(MockBehavior.Strict);

            cacheService
                .Setup(s => s.GetItem(
                    It.Is<DataBlockTableResultCacheKey>(key => key.Key == cacheKey),
                    It.IsAny<Func<Task<Either<ActionResult, TableBuilderResultViewModel>>>>()))
                .ReturnsAsync(tableResult);

            var service = BuildDataBlockService(
                contentDbContext,
                blobCacheService: cacheService.Object,
                tableBuilderService: tableBuilderService.Object
            );

            var result = (await service.GetDataBlockTableResult(releaseContentBlock)).AssertRight();

            Assert.Single(result.Results);

            VerifyAllMocks(cacheService, tableBuilderService);
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
                    Slug = "release-slug",
                    Publication = new Publication
                    {
                        Slug = "publication-slug"
                    }
                },
                ContentBlockId = _dataBlockId,
                ContentBlock = new HtmlBlock(),
            };

            await using var contentDbContext = ContentDbUtils.InMemoryContentDbContext();

            await contentDbContext.AddAsync(releaseContentBlock);
            await contentDbContext.SaveChangesAsync();

            var service = BuildDataBlockService(contentDbContext);

            var result = await service.GetDataBlockTableResult(releaseContentBlock);

            result.AssertNotFound();
        }

        private static DataBlockService BuildDataBlockService(
            ContentDbContext contentDbContext,
            IBlobCacheService blobCacheService = null,
            ITableBuilderService tableBuilderService = null,
            IUserService userService = null)
        {
            return new DataBlockService(
                contentDbContext,
                blobCacheService ?? new Mock<IBlobCacheService>().Object,
                tableBuilderService ?? new Mock<ITableBuilderService>().Object,
                userService ?? AlwaysTrueUserService().Object
            );
        }
    }
}
