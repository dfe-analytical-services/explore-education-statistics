#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils.StorageDataSetTestUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public abstract class StorageDataSetResolverTests
{
    public class ResolveTests : StorageDataSetResolverTests
    {
        [Fact]
        public async Task DataFileIsStatsDb_ReturnsStatisticsDbDataSet()
        {
            var subjectId = Guid.NewGuid();

            var contentDbContextId = await SeedDataFile(subjectId, DataStorageVersion.StatsDB);

            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var result = await resolver.Resolve(subjectId);

            var dataSet = Assert.IsType<StatisticsDbDataSet>(result);
            Assert.Equal(subjectId, dataSet.SubjectId);
        }

        [Fact]
        public async Task DataFileIsUnsupportedVersion_Throws()
        {
            var subjectId = Guid.NewGuid();

            var contentDbContextId = await SeedDataFile(subjectId, DataStorageVersion.ParquetV1);

            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var exception = await Assert.ThrowsAsync<NotSupportedException>(() => resolver.Resolve(subjectId));
            Assert.Contains("ParquetV1", exception.Message);
        }

        [Fact]
        public async Task NoDataFile_Throws()
        {
            var subjectId = Guid.NewGuid();

            await using var contentDbContext = InMemoryContentDbContext();
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.Resolve(subjectId));
            Assert.Contains(subjectId.ToString(), exception.Message);
        }

        private static async Task<string> SeedDataFile(Guid subjectId, DataStorageVersion dataStorageVersion)
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
                    DataStorageVersion = dataStorageVersion,
                }
            );
            await contentDbContext.SaveChangesAsync();

            return contentDbContextId;
        }
    }
}
