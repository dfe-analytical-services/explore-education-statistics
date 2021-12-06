#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    [Collection(BlobCacheServiceTests)]
    public class DataBlockServiceTests : BlobCacheServiceTestFixture
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

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };

            var (service, tableBuilderService) = BuildServiceAndDependencies(contentDbContext);

            CacheService
                .Setup(s => s.GetItem(
                    It.IsAny<DataBlockTableResultCacheKey>(), typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);
            
            tableBuilderService
                .Setup(
                    s =>
                        s.Query(
                            _releaseId,
                            It.Is<ObservationQueryContext>(q => q.SubjectId == subjectId),
                            default
                        )
                )
                .ReturnsAsync(tableBuilderResults);

            CacheService
                .Setup(s => s.SetItem<object>(
                    It.IsAny<DataBlockTableResultCacheKey>(),
                    tableBuilderResults))
                .Returns(Task.CompletedTask);
            
            var result = await service.GetDataBlockTableResult(releaseContentBlock);
            VerifyAllMocks(tableBuilderService, CacheService);

            result.AssertRight(tableBuilderResults);
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

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };

            var (service, tableBuilderService) = BuildServiceAndDependencies(contentDbContext);
    
            CacheService
                .Setup(s => s.GetItem(
                    It.IsAny<DataBlockTableResultCacheKey>(), typeof(TableBuilderResultViewModel)))
                .ReturnsAsync(null);
            
            tableBuilderService
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
                    It.IsAny<DataBlockTableResultCacheKey>(),
                    tableBuilderResults))
                .Returns(Task.CompletedTask);

            var result = await service.GetDataBlockTableResult(releaseContentBlock);
            VerifyAllMocks(tableBuilderService, CacheService);

            result.AssertRight(tableBuilderResults);
        }

        [Fact]
        public async Task GetDataBlockTableResult_NotDataBlockType()
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

            var (service, _) = BuildServiceAndDependencies(contentDbContext);

            var exception = await Assert.ThrowsAsync<TargetInvocationException>(() => 
                service.GetDataBlockTableResult(releaseContentBlock));

            Assert.IsType<ArgumentException>(exception.InnerException);
        }

        private static (
            DataBlockService service, 
            Mock<ITableBuilderService> tableBuilderService) 
            BuildServiceAndDependencies(ContentDbContext contentDbContext)
        {
            var tableBuilderService = new Mock<ITableBuilderService>(Strict);
            var userService = AlwaysTrueUserService();
            
            var controller = new DataBlockService(
                contentDbContext,
                tableBuilderService.Object,
                userService.Object
            );

            return (controller, tableBuilderService);
        }
    }
}
