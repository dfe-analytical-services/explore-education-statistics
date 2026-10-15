#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils.StorageDataSetTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.StatsDb;

public abstract class StatisticsDbDataSetWriterTests
{
    private readonly string _statisticsDbContextId = Guid.NewGuid().ToString();

    public class WriteFiltersAndIndicatorsTests : StatisticsDbDataSetWriterTests
    {
        [Fact]
        public async Task FiltersAndIndicatorsWrittenWithIndicatorGroups()
        {
            var subjectId = Guid.NewGuid();

            var filters = new List<Filter>
            {
                new() { Id = Guid.NewGuid(), SubjectId = subjectId },
                new() { Id = Guid.NewGuid(), SubjectId = subjectId },
            };

            var indicatorGroup = new IndicatorGroup("Group", subjectId);
            var indicators = new List<Indicator>
            {
                new() { Id = Guid.NewGuid(), IndicatorGroup = indicatorGroup },
                new() { Id = Guid.NewGuid(), IndicatorGroup = indicatorGroup },
            };

            await BuildWriter(subjectId).WriteFiltersAndIndicators(filters, indicators);

            await using var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId);

            Assert.Equal(
                filters.Select(f => f.Id).Order(),
                await statisticsDbContext.Filter.Select(f => f.Id).OrderBy(id => id).ToListAsync()
            );

            var savedIndicatorGroup = Assert.Single(
                await statisticsDbContext.IndicatorGroup.Include(ig => ig.Indicators).ToListAsync()
            );
            Assert.Equal(subjectId, savedIndicatorGroup.SubjectId);
            Assert.Equal(
                indicators.Select(i => i.Id).Order(),
                savedIndicatorGroup.Indicators.Select(i => i.Id).Order()
            );
        }
    }

    public class WriteFilterItemsTests : StatisticsDbDataSetWriterTests
    {
        [Fact]
        public async Task FilterItemsWrittenWithFilterGroupsAndAutoSelectFilterItemsSet()
        {
            var subjectId = Guid.NewGuid();

            var filterWithAutoSelect = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                AutoSelectFilterItemLabel = "total",
            };
            var filterWithoutAutoSelect = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subjectId,
                AutoSelectFilterItemLabel = "Missing",
            };

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
            {
                statisticsDbContext.Filter.AddRange(filterWithAutoSelect, filterWithoutAutoSelect);
                await statisticsDbContext.SaveChangesAsync();
            }

            var group1 = new FilterGroup(filterWithAutoSelect.Id, "Group 1");
            var group2 = new FilterGroup(filterWithoutAutoSelect.Id, "Group 2");
            var totalItem = new FilterItem("Total", group1);
            var filterItems = new List<FilterItem> { totalItem, new("Item", group1), new("Item", group2) };

            await BuildWriter(subjectId)
                .WriteFilterItems(
                    filterItems,
                    new Dictionary<Guid, FilterItem?>
                    {
                        { filterWithAutoSelect.Id, totalItem },
                        { filterWithoutAutoSelect.Id, null },
                    }
                );

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
            {
                var filters = await statisticsDbContext
                    .Filter.Include(f => f.FilterGroups)
                        .ThenInclude(fg => fg.FilterItems)
                    .ToDictionaryAsync(f => f.Id);

                var savedFilterWithAutoSelect = filters[filterWithAutoSelect.Id];
                Assert.Equal(totalItem.Id, savedFilterWithAutoSelect.AutoSelectFilterItemId);
                Assert.Equal("Total", savedFilterWithAutoSelect.AutoSelectFilterItemLabel);
                var savedGroup1 = Assert.Single(savedFilterWithAutoSelect.FilterGroups);
                Assert.Equal(group1.Id, savedGroup1.Id);
                Assert.Equal(2, savedGroup1.FilterItems.Count);

                var savedFilterWithoutAutoSelect = filters[filterWithoutAutoSelect.Id];
                Assert.Null(savedFilterWithoutAutoSelect.AutoSelectFilterItemId);
                Assert.Null(savedFilterWithoutAutoSelect.AutoSelectFilterItemLabel);
                var savedGroup2 = Assert.Single(savedFilterWithoutAutoSelect.FilterGroups);
                Assert.Single(savedGroup2.FilterItems);
            }
        }
    }

    public class WriteLocationsTests : StatisticsDbDataSetWriterTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly Country _wales = new("W92000004", "Wales");

        [Fact]
        public async Task OnlyNewLocationsWritten_AndAllLocationIdsAvailable()
        {
            var existingLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england,
            };

            var importerLocationCache = new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>());

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
            {
                statisticsDbContext.Location.Add(existingLocation);
                await statisticsDbContext.SaveChangesAsync();

                importerLocationCache.LoadLocations(statisticsDbContext);
            }

            var writer = BuildWriter(Guid.NewGuid(), importerLocationCache: importerLocationCache);

            await writer.WriteLocations([
                new Location { GeographicLevel = GeographicLevel.Country, Country = _england },
                new Location { GeographicLevel = GeographicLevel.Country, Country = _wales },
            ]);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
            {
                var locations = await statisticsDbContext.Location.ToListAsync();
                Assert.Equal(2, locations.Count);

                var newLocation = Assert.Single(locations, l => l.Id != existingLocation.Id);
                Assert.Equal(_wales, newLocation.Country);

                Assert.Equal(
                    existingLocation.Id,
                    writer.GetLocationId(new Location { GeographicLevel = GeographicLevel.Country, Country = _england })
                );
                Assert.Equal(
                    newLocation.Id,
                    writer.GetLocationId(new Location { GeographicLevel = GeographicLevel.Country, Country = _wales })
                );
            }
        }
    }

    public class WriteObservationsTests : StatisticsDbDataSetWriterTests
    {
        [Fact]
        public async Task ObservationsPassedToBatchImporter()
        {
            var observations = new List<Observation>
            {
                new() { Id = Guid.NewGuid() },
                new() { Id = Guid.NewGuid() },
            };

            var observationBatchImporter = new Mock<IObservationBatchImporter>(Strict);
            observationBatchImporter
                .Setup(i => i.ImportObservationBatch(It.IsAny<StatisticsDbContext>(), observations))
                .Returns(Task.CompletedTask);

            await BuildWriter(Guid.NewGuid(), observationBatchImporter: observationBatchImporter.Object)
                .WriteObservations(observations);

            VerifyAllMocks(observationBatchImporter);
        }
    }

    public class CountObservationsTests : StatisticsDbDataSetWriterTests
    {
        [Fact]
        public async Task ObservationsForSubjectOnlyCounted()
        {
            var subjectId = Guid.NewGuid();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(_statisticsDbContextId))
            {
                statisticsDbContext.Observation.AddRange(
                    new Observation { SubjectId = subjectId },
                    new Observation { SubjectId = subjectId },
                    new Observation { SubjectId = Guid.NewGuid() }
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            Assert.Equal(2, await BuildWriter(subjectId).CountObservations());
        }
    }

    private StatisticsDbDataSetWriter BuildWriter(
        Guid subjectId,
        IImporterLocationCache? importerLocationCache = null,
        IObservationBatchImporter? observationBatchImporter = null
    )
    {
        var dbContextSupplier = new Mock<IDbContextSupplier>(Strict);
        dbContextSupplier
            .Setup(s => s.CreateDbContextDelegate<StatisticsDbContext>())
            .Returns(() => InMemoryStatisticsDbContext(_statisticsDbContextId));

        return new StatisticsDbDataSetWriter(
            subjectId: subjectId,
            context: InMemoryStatisticsDbContext(_statisticsDbContextId),
            databaseHelper: new InMemoryDatabaseHelper(dbContextSupplier.Object),
            importerLocationService: new ImporterLocationService(
                new SequentialGuidGenerator(),
                importerLocationCache ?? new ImporterLocationCache(Mock.Of<ILogger<ImporterLocationCache>>()),
                Mock.Of<ILogger<ImporterLocationCache>>()
            ),
            observationBatchImporter: observationBatchImporter ?? Mock.Of<IObservationBatchImporter>(Strict),
            statisticsDbDataSetFactory: BuildStatisticsDbDataSetFactory(
                InMemoryStatisticsDbContext(_statisticsDbContextId)
            )
        );
    }
}
