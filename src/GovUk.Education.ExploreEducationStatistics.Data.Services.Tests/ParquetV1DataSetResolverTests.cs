#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils.StorageDataSetTestUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public abstract class ParquetV1DataSetResolverTests
{
    public class ResolveTests : ParquetV1DataSetResolverTests
    {
        [Fact]
        public async Task DataFileHasParquet_ReturnsParquetV1DataSet()
        {
            var subjectId = Guid.NewGuid();

            var contentDbContextId = await SeedDataFile(subjectId, hasParquet: true);

            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildParquetV1DataSetResolver(contentDbContext, statisticsDbContext);

            var result = await resolver.Resolve(subjectId);

            var dataSet = Assert.IsType<ParquetV1DataSet>(result);
            Assert.Equal(subjectId, dataSet.SubjectId);
        }

        [Fact]
        public async Task DataFileHasNoParquet_ReturnsStatisticsDbDataSet()
        {
            var subjectId = Guid.NewGuid();

            var contentDbContextId = await SeedDataFile(subjectId, hasParquet: false);

            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildParquetV1DataSetResolver(contentDbContext, statisticsDbContext);

            var result = await resolver.Resolve(subjectId);

            var dataSet = Assert.IsType<StatisticsDbDataSet>(result);
            Assert.Equal(subjectId, dataSet.SubjectId);
        }

        [Fact]
        public async Task NoDataFile_ReturnsStatisticsDbDataSet()
        {
            var subjectId = Guid.NewGuid();

            await using var contentDbContext = InMemoryContentDbContext();
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildParquetV1DataSetResolver(contentDbContext, statisticsDbContext);

            var result = await resolver.Resolve(subjectId);

            var dataSet = Assert.IsType<StatisticsDbDataSet>(result);
            Assert.Equal(subjectId, dataSet.SubjectId);
        }

        private static async Task<string> SeedDataFile(Guid subjectId, bool hasParquet)
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

            contentDbContext.Files.Add(
                new File
                {
                    Id = Guid.NewGuid(),
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    SubjectId = subjectId,
                    Type = FileType.Data,
                    HasParquet = hasParquet,
                }
            );
            await contentDbContext.SaveChangesAsync();

            return contentDbContextId;
        }
    }

    private static ParquetV1DataSetResolver BuildParquetV1DataSetResolver(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext
    )
    {
        return new ParquetV1DataSetResolver(
            contentDbContext: contentDbContext,
            statisticsDbDataSetResolver: BuildStatisticsDbDataSetResolver(statisticsDbContext),
            dataFilesPathResolver: Mock.Of<IDataFilesPathResolver>(Strict),
            logger: Mock.Of<ILogger<ParquetV1DataSet>>()
        );
    }
}
