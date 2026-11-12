#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils.StorageDataSetTestUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests;

public class StorageDataSetWriterResolverTests
{
    [Fact]
    public async Task DataFileIsStatsDb_ReturnsStatisticsDbDataSetWriter()
    {
        var dataFile = BuildDataFile(Guid.NewGuid());

        await using var statisticsDbContext = InMemoryStatisticsDbContext();

        var result = BuildResolver(statisticsDbContext).Resolve(dataFile);

        var writer = Assert.IsType<StatisticsDbDataSetWriter>(result);
        Assert.Equal(dataFile.SubjectId, writer.SubjectId);
    }

    [Fact]
    public async Task FileIsNotDataFile_Throws()
    {
        var file = BuildDataFile(Guid.NewGuid());
        file.Type = FileType.Metadata;

        await using var statisticsDbContext = InMemoryStatisticsDbContext();

        var exception = Assert.Throws<ArgumentException>(() => BuildResolver(statisticsDbContext).Resolve(file));
        Assert.Contains(file.Id.ToString(), exception.Message);
    }

    [Fact]
    public async Task FileHasNoSubject_Throws()
    {
        var file = BuildDataFile(Guid.NewGuid());
        file.SubjectId = null;

        await using var statisticsDbContext = InMemoryStatisticsDbContext();

        Assert.Throws<ArgumentException>(() => BuildResolver(statisticsDbContext).Resolve(file));
    }

    [Fact]
    public async Task FileHasNoDataStorageVersion_Throws()
    {
        var file = BuildDataFile(Guid.NewGuid());
        file.DataStorageVersion = null;

        await using var statisticsDbContext = InMemoryStatisticsDbContext();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            BuildResolver(statisticsDbContext).Resolve(file)
        );
        Assert.Contains(file.Id.ToString(), exception.Message);
    }

    private static StorageDataSetWriterResolver BuildResolver(StatisticsDbContext statisticsDbContext)
    {
        return BuildStorageDataSetWriterResolver(statisticsDbContext, Mock.Of<IDatabaseHelper>(Strict));
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
