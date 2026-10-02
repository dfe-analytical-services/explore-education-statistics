#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils.StorageDataSetTestUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests;

public abstract class StorageDataSetResolverTests
{
    public class ResolveTests : StorageDataSetResolverTests
    {
        [Fact]
        public async Task DataFileIsStatsDb_ReturnsStatisticsDbDataSet()
        {
            var subjectId = Guid.NewGuid();

            var contentDbContextId = await SeedDataFile(subjectId);

            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var result = await resolver.Resolve(subjectId);

            var dataSet = Assert.IsType<StatisticsDbDataSet>(result);
            Assert.Equal(subjectId, dataSet.SubjectId);
        }

        [Fact]
        public async Task SameSubjectResolvedTwice_ReturnsSameInstance()
        {
            var subjectId = Guid.NewGuid();

            var contentDbContextId = await SeedDataFile(subjectId);

            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var first = await resolver.Resolve(subjectId);
            var second = await resolver.Resolve(subjectId);

            Assert.Same(first, second);
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
    }

    public class ResolveFileTests : StorageDataSetResolverTests
    {
        [Fact]
        public async Task DataFileIsStatsDb_ReturnsStatisticsDbDataSetWithoutQueryingFiles()
        {
            var dataFile = BuildDataFile(Guid.NewGuid());

            // Nothing is seeded, so a Files lookup would find no data file
            await using var contentDbContext = InMemoryContentDbContext();
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var result = resolver.Resolve(dataFile);

            var dataSet = Assert.IsType<StatisticsDbDataSet>(result);
            Assert.Equal(dataFile.SubjectId, dataSet.SubjectId);
        }

        [Fact]
        public async Task SameSubjectResolvedByFileAndId_ReturnsSameInstance()
        {
            var subjectId = Guid.NewGuid();

            var contentDbContextId = await SeedDataFile(subjectId);

            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var byFile = resolver.Resolve(BuildDataFile(subjectId));
            var byId = await resolver.Resolve(subjectId);

            Assert.Same(byFile, byId);
        }

        [Fact]
        public async Task FileIsNotDataFile_Throws()
        {
            var file = BuildDataFile(Guid.NewGuid());
            file.Type = FileType.Metadata;

            await using var contentDbContext = InMemoryContentDbContext();
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var exception = Assert.Throws<ArgumentException>(() => resolver.Resolve(file));
            Assert.Contains(file.Id.ToString(), exception.Message);
        }

        [Fact]
        public async Task FileHasNoSubject_Throws()
        {
            var file = BuildDataFile(Guid.NewGuid());
            file.SubjectId = null;

            await using var contentDbContext = InMemoryContentDbContext();
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            Assert.Throws<ArgumentException>(() => resolver.Resolve(file));
        }

        [Fact]
        public async Task FileHasNoDataStorageVersion_Throws()
        {
            var file = BuildDataFile(Guid.NewGuid());
            file.DataStorageVersion = null;

            await using var contentDbContext = InMemoryContentDbContext();
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var exception = Assert.Throws<InvalidOperationException>(() => resolver.Resolve(file));
            Assert.Contains(file.Id.ToString(), exception.Message);
        }
    }

    public class TryResolveTests : StorageDataSetResolverTests
    {
        [Fact]
        public async Task DataFileIsStatsDb_ReturnsStatisticsDbDataSet()
        {
            var subjectId = Guid.NewGuid();

            var contentDbContextId = await SeedDataFile(subjectId);

            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var result = await resolver.TryResolve(subjectId);

            var dataSet = Assert.IsType<StatisticsDbDataSet>(result);
            Assert.Equal(subjectId, dataSet.SubjectId);
        }

        [Fact]
        public async Task NoDataFile_ReturnsNull()
        {
            var subjectId = Guid.NewGuid();

            await using var contentDbContext = InMemoryContentDbContext();
            await using var statisticsDbContext = InMemoryStatisticsDbContext();

            var resolver = BuildStorageDataSetResolver(contentDbContext, statisticsDbContext);

            var result = await resolver.TryResolve(subjectId);

            Assert.Null(result);
        }
    }

    private static async Task<string> SeedDataFile(Guid subjectId)
    {
        var contentDbContextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);

        contentDbContext.Files.Add(BuildDataFile(subjectId));
        await contentDbContext.SaveChangesAsync();

        return contentDbContextId;
    }

    private static File BuildDataFile(Guid subjectId)
    {
        return new File
        {
            Id = Guid.NewGuid(),
            RootPath = Guid.NewGuid(),
            Filename = "data.csv",
            SubjectId = subjectId,
            Type = FileType.Data,
            DataStorageVersion = DataStorageVersion.StatsDB,
        };
    }
}
