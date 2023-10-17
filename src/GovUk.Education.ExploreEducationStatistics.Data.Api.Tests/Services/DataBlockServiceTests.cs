#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services
{
    public class DataBlockServiceTests
    {
        [Fact]
        public async Task GetDataBlockTableResult()
        {
            var subjectId = Guid.NewGuid();

            var release = new Release();

            var dataBlock = new DataBlock
            {
                Query = new ObservationQueryContext
                {
                    SubjectId = subjectId
                },
                Release = release
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.ContentBlocks.AddRangeAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            var tableBuilderResults = new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new()
                }
            };

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var (service, tableBuilderService) = BuildServiceAndDependencies(contentDbContext);

                tableBuilderService
                    .Setup(
                        s =>
                            s.Query(
                                dataBlock.ReleaseId,
                                It.Is<ObservationQueryContext>(q => q.SubjectId == subjectId),
                                default
                            )
                    )
                    .ReturnsAsync(tableBuilderResults);

                var result = (await service.GetDataBlockTableResult(
                    dataBlock.ReleaseId,
                    dataBlock.Id)).AssertRight();

                VerifyAllMocks(tableBuilderService);

                Assert.Equal(tableBuilderResults, result);
            }
        }

        [Fact]
        public async Task GetDataBlockTableResult_NotDataBlockType()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            var htmlBlock = new HtmlBlock
            {
                Release = new Release()
            };
            
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.ContentBlocks.AddRangeAsync(htmlBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var (service, _) = BuildServiceAndDependencies(contentDbContext);

                var result = await service.GetDataBlockTableResult(
                    htmlBlock.ReleaseId,
                    htmlBlock.Id);

                result.AssertNotFound();
            }
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
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                tableBuilderService.Object,
                userService.Object
            );

            return (controller, tableBuilderService);
        }
    }
}
