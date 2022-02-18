#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class DataBlockMigrationServiceTests
    {
        private readonly Subject _subject = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task Migrate_DataBlockNotFound()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext);

                var result = await service.Migrate(Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Migrate_SubjectNotFound()
        {
            var dataBlock = new DataBlock
            {
                Query = new ObservationQueryContext
                {
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            var statisticsPersistenceHelper = new Mock<IPersistenceHelper<StatisticsDbContext>>(MockBehavior.Strict);
            statisticsPersistenceHelper.Setup(mock =>
                    mock.CheckEntityExists(
                        dataBlock.Query.SubjectId,
                        It.IsAny<Func<IQueryable<Subject>, IQueryable<Subject>>>()))
                .ReturnsAsync(new NotFoundResult());

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    statisticsPersistenceHelper: statisticsPersistenceHelper.Object);

                var result = await service.Migrate(dataBlock.Id);

                MockUtils.VerifyAllMocks(statisticsPersistenceHelper);

                result.AssertNotFound();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                Assert.False(after.LocationsMigrated);
            }
        }

        [Fact]
        public async Task Migrate_DataBlockHasNoLocations()
        {
            var dataBlock = new DataBlock
            {
                Charts = new List<IChart>(),
                Table = new TableBuilderConfiguration(),
                Query = new ObservationQueryContext
                {
                    SubjectId = _subject.Id,
                    Locations = new LocationQuery()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.DataBlocks.AddAsync(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            var locationRepository = new Mock<ILocationRepository>(MockBehavior.Strict);
            locationRepository.Setup(mock => mock.GetDistinctForSubject(_subject.Id))
                .ReturnsAsync(new List<Location>());

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(
                    contentDbContext: contentDbContext,
                    locationRepository: locationRepository.Object);

                var result = await service.Migrate(dataBlock.Id);

                MockUtils.VerifyAllMocks(locationRepository);

                result.AssertRight();
            }

            await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contentDbContextId))
            {
                var after = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                VerifyOriginalFieldsAreUntouched(dataBlock, after);
                Assert.True(after.LocationsMigrated);

                Assert.Empty(after.ChartsMigrated);
                Assert.Empty(after.QueryMigrated.LocationIds);
                Assert.Empty(after.TableMigrated.TableHeaders.Columns);
                Assert.Empty(after.TableMigrated.TableHeaders.Rows);
                Assert.Empty(after.TableMigrated.TableHeaders.ColumnGroups);
                Assert.Empty(after.TableMigrated.TableHeaders.RowGroups);
            }
        }

        // TODO EES-3167 Write some more tests! :)

        private DataBlockMigrationService BuildService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
            ILocationRepository? locationRepository = null)
        {
            return new DataBlockMigrationService(
                contentDbContext,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                statisticsPersistenceHelper ?? DefaultStatisticsPersistenceHelper(),
                locationRepository ?? Mock.Of<ILocationRepository>(MockBehavior.Strict));
        }

        private static void VerifyOriginalFieldsAreUntouched(DataBlock original, DataBlock after)
        {
            // Check the original data block fields have not been touched
            after.Charts.AssertDeepEqualTo(original.Charts);
            after.Query.AssertDeepEqualTo(original.Query);
            after.Table.AssertDeepEqualTo(original.Table);
        }

        private IPersistenceHelper<StatisticsDbContext> DefaultStatisticsPersistenceHelper()
        {
            var statisticsPersistenceHelper = MockUtils.MockPersistenceHelper<StatisticsDbContext, Subject>();
            MockUtils.SetupCall(statisticsPersistenceHelper, _subject.Id, _subject);
            return statisticsPersistenceHelper.Object;
        }
    }
}
