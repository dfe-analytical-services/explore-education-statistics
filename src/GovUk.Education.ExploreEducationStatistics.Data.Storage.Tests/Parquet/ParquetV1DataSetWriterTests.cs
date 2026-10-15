#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils.StorageDataSetTestUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Parquet;

public abstract class ParquetV1DataSetWriterTests
{
    private readonly Guid _subjectId = Guid.NewGuid();
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();
    private readonly Mock<IParquetDataSetWriter> _parquetDataSetWriter = new(Strict);

    public class WriteFiltersAndIndicatorsTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task WrittenToStatisticsDb()
        {
            var filter = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = _subjectId,
                Label = "Filter",
            };
            var indicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator",
                IndicatorGroup = new IndicatorGroup
                {
                    Id = Guid.NewGuid(),
                    SubjectId = _subjectId,
                    Label = "Group",
                },
            };

            await BuildWriter().WriteFiltersAndIndicators([filter], [indicator]);

            await using var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId);

            Assert.Equal(filter.Id, Assert.Single(await statisticsDbContext.Filter.ToListAsync()).Id);
            Assert.Equal(indicator.Id, Assert.Single(await statisticsDbContext.Indicator.ToListAsync()).Id);
        }
    }

    public class StartObservationsTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task StagingStarted()
        {
            List<Filter> filters = [new() { Id = Guid.NewGuid() }];
            List<Indicator> indicators = [new() { Id = Guid.NewGuid() }];

            _parquetDataSetWriter
                .Setup(w => w.StartStaging(_subjectId, filters, indicators, 10, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await BuildWriter().StartObservations(filters, indicators, resumeFromCsvRow: 10);

            VerifyAllMocks(_parquetDataSetWriter);
        }
    }

    public class WriteObservationsTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task ObservationsStagedWithFiltersAndIndicators()
        {
            List<Filter> filters = [new() { Id = Guid.NewGuid() }];
            List<Indicator> indicators = [new() { Id = Guid.NewGuid() }];
            List<Observation> observations = [new() { Id = Guid.NewGuid() }];

            _parquetDataSetWriter
                .Setup(w => w.StartStaging(_subjectId, filters, indicators, null, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _parquetDataSetWriter
                .Setup(w =>
                    w.AppendObservations(_subjectId, filters, indicators, observations, It.IsAny<CancellationToken>())
                )
                .Returns(Task.CompletedTask);

            var writer = BuildWriter();
            await writer.StartObservations(filters, indicators, resumeFromCsvRow: null);
            await writer.WriteObservations(observations);

            VerifyAllMocks(_parquetDataSetWriter);
        }

        [Fact]
        public async Task ObservationsNotStarted_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                BuildWriter().WriteObservations([new Observation { Id = Guid.NewGuid() }])
            );
        }
    }

    public class CountObservationsTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task StagedObservationsCounted()
        {
            _parquetDataSetWriter
                .Setup(w => w.CountStagedObservations(_subjectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            Assert.Equal(3, await BuildWriter().CountObservations());
        }
    }

    public class CompleteTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task ParquetFilesWrittenWithMetaFromStatisticsDb()
        {
            var filterItem = new FilterItem { Id = Guid.NewGuid(), Label = "Item" };
            var filter = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = _subjectId,
                Label = "Filter",
                FilterGroups =
                [
                    new FilterGroup
                    {
                        Id = Guid.NewGuid(),
                        Label = "Group",
                        FilterItems = [filterItem],
                    },
                ],
            };
            var indicatorGroup = new IndicatorGroup
            {
                Id = Guid.NewGuid(),
                SubjectId = _subjectId,
                Label = "Group",
                Indicators = [new Indicator { Id = Guid.NewGuid(), Label = "Indicator" }],
            };
            var stagedLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = new Country("E92000001", "England"),
            };
            var otherLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = new Country("W92000004", "Wales"),
            };

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
            {
                statisticsDbContext.Filter.Add(filter);
                statisticsDbContext.IndicatorGroup.Add(indicatorGroup);
                statisticsDbContext.Location.AddRange(stagedLocation, otherLocation);
                await statisticsDbContext.SaveChangesAsync();
            }

            _parquetDataSetWriter
                .Setup(w => w.ListStagedLocationIds(_subjectId, It.IsAny<CancellationToken>()))
                .ReturnsAsync([stagedLocation.Id]);
            _parquetDataSetWriter
                .Setup(w =>
                    w.WriteParquetFiles(
                        _subjectId,
                        It.Is<IReadOnlyList<Filter>>(filters =>
                            filters.Single().Id == filter.Id
                            && filters.Single().FilterGroups.Single().FilterItems.Single().Id == filterItem.Id
                        ),
                        It.Is<IReadOnlyList<IndicatorGroup>>(groups =>
                            groups.Single().Id == indicatorGroup.Id && groups.Single().Indicators.Count == 1
                        ),
                        It.Is<IReadOnlyList<Location>>(locations => locations.Single().Id == stagedLocation.Id),
                        It.IsAny<CancellationToken>()
                    )
                )
                .Returns(Task.CompletedTask);

            await BuildWriter().Complete();

            VerifyAllMocks(_parquetDataSetWriter);
        }
    }

    private ParquetV1DataSetWriter BuildWriter()
    {
        var dbContextSupplier = new Mock<IDbContextSupplier>(Strict);
        dbContextSupplier
            .Setup(s => s.CreateDbContextDelegate<StatisticsDbContext>())
            .Returns(() => InMemoryStatisticsDbContext(_statisticsDbContextId));

        var resolver = BuildStorageDataSetWriterResolver(
            InMemoryStatisticsDbContext(_statisticsDbContextId),
            new InMemoryDatabaseHelper(dbContextSupplier.Object),
            parquetDataSetWriter: _parquetDataSetWriter.Object
        );

        var dataFile = new File
        {
            Id = Guid.NewGuid(),
            RootPath = Guid.NewGuid(),
            Filename = "data.csv",
            SubjectId = _subjectId,
            Type = FileType.Data,
            DataStorageVersion = DataStorageVersion.ParquetV1,
        };

        return Assert.IsType<ParquetV1DataSetWriter>(resolver.Resolve(dataFile));
    }
}
