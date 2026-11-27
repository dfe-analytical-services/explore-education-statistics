#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Parquet;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils.StorageDataSetTestUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Tests.Services;

public abstract class ParquetDataSetServiceTests : IDisposable
{
    private readonly string _contentDbContextId = Guid.NewGuid().ToString();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();

    private readonly ParquetTestDataSet _testDataSet = new();

    public void Dispose() => _testDataSet.Dispose();

    public class ExportStatsDbDataSetTests : ParquetDataSetServiceTests
    {
        [Fact]
        public async Task WritesParquetFilesAndSetsHasParquet()
        {
            // Older imports didn't record the filter of each observation's filter items.
            _testDataSet.Observations[0].FilterItems.ForEach(item => item.FilterId = null);

            var file = await SeedDataSet(hasParquet: false);

            await BuildService(_testDataSet.BuildWriter()).ExportStatsDbDataSet(file.Id);

            await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
            {
                var savedFile = contentDbContext.Files.Single(f => f.Id == file.Id);

                Assert.True(savedFile.HasParquet);
                Assert.Equal(DataStorageVersion.StatsDB, savedFile.DataStorageVersion);
            }

            Assert.False(System.IO.File.Exists(_testDataSet.PathResolver.StagingDbPath(_testDataSet.SubjectId)));
            Assert.False(System.IO.File.Exists(ExportLockPath()));

            var observations = await _testDataSet.BuildDataSet().ListObservations([], [], null);

            Assert.Equal(_testDataSet.Observations.Count, observations.Count);

            var firstObservation = Assert.Single(observations, o => o.Id == _testDataSet.Observations[0].Id);
            Assert.Equal(
                new[] { _testDataSet.Primary.Id, _testDataSet.Year1.Id }.Order(),
                firstObservation.FilterItems.Select(item => item.FilterItemId).Order()
            );
            Assert.Equal(
                new[] { _testDataSet.SchoolType.Id, _testDataSet.NcYear.Id }.Order(),
                firstObservation.FilterItems.Select(item => item.FilterId!.Value).Order()
            );
        }

        [Fact]
        public async Task AlreadyHasParquet_DoesNothing()
        {
            var file = await SeedDataSet(hasParquet: true);

            await BuildService(Mock.Of<IParquetDataSetWriter>(Strict)).ExportStatsDbDataSet(file.Id);

            Assert.False(Directory.Exists(_testDataSet.PathResolver.DirectoryPath(_testDataSet.SubjectId)));
        }

        [Fact]
        public async Task ExportLockHeld_DoesNothing()
        {
            var file = await SeedDataSet(hasParquet: false);
            CreateExportLock();

            await BuildService(Mock.Of<IParquetDataSetWriter>(Strict)).ExportStatsDbDataSet(file.Id);

            await using var contentDbContext = InMemoryContentDbContext(_contentDbContextId);

            Assert.False(contentDbContext.Files.Single(f => f.Id == file.Id).HasParquet);
            Assert.True(System.IO.File.Exists(ExportLockPath()));
        }

        [Fact]
        public async Task StaleExportLock_ExportsAndRemovesLock()
        {
            var file = await SeedDataSet(hasParquet: false);
            CreateExportLock();
            System.IO.File.SetLastWriteTimeUtc(ExportLockPath(), DateTime.UtcNow.AddHours(-5));

            await BuildService(_testDataSet.BuildWriter()).ExportStatsDbDataSet(file.Id);

            await using var contentDbContext = InMemoryContentDbContext(_contentDbContextId);

            Assert.True(contentDbContext.Files.Single(f => f.Id == file.Id).HasParquet);
            Assert.False(System.IO.File.Exists(ExportLockPath()));
        }

        [Fact]
        public async Task ExportFails_RemovesLock()
        {
            var file = await SeedDataSet(hasParquet: false);

            await Assert.ThrowsAsync<MockException>(() =>
                BuildService(Mock.Of<IParquetDataSetWriter>(Strict)).ExportStatsDbDataSet(file.Id)
            );

            Assert.False(System.IO.File.Exists(ExportLockPath()));
        }

        [Fact]
        public async Task StagedCountDiffersFromStatsDb_ThrowsWithoutSettingHasParquet()
        {
            var file = await SeedDataSet(hasParquet: false);

            var realWriter = _testDataSet.BuildWriter();
            var writer = new Mock<IParquetDataSetWriter>();
            writer
                .Setup(w =>
                    w.WriteLocations(
                        It.IsAny<Guid>(),
                        It.IsAny<IReadOnlyList<Location>>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(
                    (Guid subjectId, IReadOnlyList<Location> locations, CancellationToken cancellationToken) =>
                        realWriter.WriteLocations(subjectId, locations, cancellationToken)
                );
            writer
                .Setup(w => w.CountStagedObservations(_testDataSet.SubjectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testDataSet.Observations.Count - 1);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                BuildService(writer.Object).ExportStatsDbDataSet(file.Id)
            );
            Assert.Equal(
                $"Staged {_testDataSet.Observations.Count - 1} observations for data file {file.Id} "
                    + $"but StatsDB has {_testDataSet.Observations.Count}",
                exception.Message
            );

            writer.Verify(
                w =>
                    w.WriteTimePeriodsAndData(
                        It.IsAny<Guid>(),
                        It.IsAny<IReadOnlyList<Filter>>(),
                        It.IsAny<IReadOnlyList<Indicator>>(),
                        It.IsAny<IReadOnlyList<Location>>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );

            await using var contentDbContext = InMemoryContentDbContext(_contentDbContextId);
            Assert.False(contentDbContext.Files.Single(f => f.Id == file.Id).HasParquet);
            Assert.False(System.IO.File.Exists(ExportLockPath()));
        }

        [Fact]
        public async Task NotStoredInStatsDb_Throws()
        {
            var file = await SeedDataSet(hasParquet: false, DataStorageVersion.ParquetV1);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                BuildService(Mock.Of<IParquetDataSetWriter>(Strict)).ExportStatsDbDataSet(file.Id)
            );
        }
    }

    private async Task<File> SeedDataSet(
        bool hasParquet,
        DataStorageVersion dataStorageVersion = DataStorageVersion.StatsDB
    )
    {
        var file = new File
        {
            Id = Guid.NewGuid(),
            RootPath = Guid.NewGuid(),
            Filename = "data.csv",
            Type = FileType.Data,
            SubjectId = _testDataSet.SubjectId,
            DataStorageVersion = dataStorageVersion,
            HasParquet = hasParquet,
        };

        await using (var contentDbContext = InMemoryContentDbContext(_contentDbContextId))
        {
            contentDbContext.Files.Add(file);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
        {
            statisticsDbContext.Filter.AddRange(_testDataSet.Filters);
            statisticsDbContext.IndicatorGroup.AddRange(_testDataSet.IndicatorGroups);
            statisticsDbContext.Location.AddRange(_testDataSet.Locations);
            statisticsDbContext.Observation.AddRange(_testDataSet.Observations);
            await statisticsDbContext.SaveChangesAsync();
        }

        return file;
    }

    private string ExportLockPath() =>
        Path.Combine(_testDataSet.PathResolver.DirectoryPath(_testDataSet.SubjectId), "export.lock");

    private void CreateExportLock()
    {
        Directory.CreateDirectory(_testDataSet.PathResolver.DirectoryPath(_testDataSet.SubjectId));
        System.IO.File.WriteAllText(ExportLockPath(), "");
    }

    private ParquetDataSetService BuildService(IParquetDataSetWriter writer)
    {
        var dbContextSupplier = new InMemoryDbContextSupplier(
            contentDbContextId: _contentDbContextId,
            statisticsDbContextId: _statisticsDbContextId
        );

        return new ParquetDataSetService(
            dbContextSupplier,
            BuildStatisticsDbDataSetFactory(InMemoryStatisticsDbContext(_statisticsDbContextId)),
            BuildParquetV1DataSetWriterFactory(
                InMemoryStatisticsDbContext(_statisticsDbContextId),
                new InMemoryDatabaseHelper(dbContextSupplier),
                writer,
                _testDataSet.PathResolver
            ),
            _testDataSet.PathResolver,
            Mock.Of<ILogger<ParquetDataSetService>>()
        );
    }
}
