#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Utils.StorageDataSetTestUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Parquet;

public abstract class ParquetV1DataSetWriterTests : IDisposable
{
    private readonly ParquetTestDataSet _testDataSet = new();

    public void Dispose() => _testDataSet.Dispose();

    public class WriteIndicatorsTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task WrittenToParquetAndReadableFromDataSet()
        {
            var writer = BuildWriter();

            Assert.False(await writer.HasIndicators());

            await writer.WriteIndicators(_testDataSet.Indicators);

            Assert.True(await writer.HasIndicators());

            var indicatorGroups = await writer.DataSet.ListIndicatorGroups();

            Assert.Equal(
                _testDataSet.IndicatorGroups.Select(group => group.Id).Order(),
                indicatorGroups.Select(group => group.Id).Order()
            );
            Assert.Equal(
                _testDataSet.Indicators.Select(indicator => indicator.Id).Order(),
                indicatorGroups.SelectMany(group => group.Indicators).Select(indicator => indicator.Id).Order()
            );
        }
    }

    public class WriteFiltersTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task WrittenToParquetAndReadableFromDataSet()
        {
            var writer = BuildWriter();

            Assert.False(await writer.HasFilters());

            await writer.WriteFilters(_testDataSet.Filters);

            Assert.True(await writer.HasFilters());

            var filters = await writer.DataSet.ListFilters(includeItems: true);

            var schoolType = Assert.Single(filters, filter => filter.Id == _testDataSet.SchoolType.Id);
            Assert.Equal(_testDataSet.Primary.Id, schoolType.AutoSelectFilterItemId);
            Assert.Equal(_testDataSet.Primary.Label, schoolType.AutoSelectFilterItemLabel);

            Assert.Equal(
                _testDataSet
                    .Filters.SelectMany(filter => filter.FilterGroups)
                    .SelectMany(group => group.FilterItems)
                    .Select(item => item.Id)
                    .Order(),
                filters
                    .SelectMany(filter => filter.FilterGroups)
                    .SelectMany(group => group.FilterItems)
                    .Select(item => item.Id)
                    .Order()
            );
        }
    }

    public class WriteLocationsTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task LocationsWithIds_IdsKept()
        {
            var writer = BuildWriter();

            await writer.WriteLocations(_testDataSet.Locations);

            Assert.Equal(
                _testDataSet.Locations.Select(location => location.Id).Order(),
                (await writer.DataSet.ListLocations()).Select(location => location.Id).Order()
            );
        }

        [Fact]
        public async Task LocationsWithoutIds_GivenIds()
        {
            var location = new Location
            {
                GeographicLevel = GeographicLevel.Country,
                Country = new Country("E92000001", "England"),
            };

            var writer = BuildWriter();

            await writer.WriteLocations([location]);

            Assert.NotEqual(Guid.Empty, location.Id);
            Assert.Equal(location.Id, Assert.Single(await writer.DataSet.ListLocations()).Id);
        }
    }

    public class GetLocationIdTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task ReturnsIdOfWrittenLocationWithSameAttributes()
        {
            var writer = BuildWriter();

            await writer.WriteLocations(_testDataSet.Locations);
            await writer.StartObservations(_testDataSet.Filters, _testDataSet.Indicators, resumeFromCsvRow: null);

            var locationFromCsv = new Location
            {
                GeographicLevel = GeographicLevel.Country,
                Country = new Country("E92000001", "England"),
            };

            Assert.Equal(_testDataSet.England.Id, writer.GetLocationId(locationFromCsv));
        }

        [Fact]
        public void ObservationsNotStarted_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => BuildWriter().GetLocationId(_testDataSet.England));
        }
    }

    public class WriteObservationsTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task ObservationsNotStarted_Throws()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                BuildWriter().WriteObservations([new Observation { Id = Guid.NewGuid() }])
            );
        }
    }

    public class CompleteTests : ParquetV1DataSetWriterTests
    {
        [Fact]
        public async Task ObservationsReadableFromDataSet()
        {
            var writer = BuildWriter();

            await writer.WriteIndicators(_testDataSet.Indicators);
            await writer.WriteFilters(_testDataSet.Filters);
            await writer.WriteLocations(_testDataSet.Locations);
            await writer.StartObservations(_testDataSet.Filters, _testDataSet.Indicators, resumeFromCsvRow: null);
            await writer.WriteObservations(_testDataSet.Observations);

            Assert.Equal(_testDataSet.Observations.Count, await writer.CountObservations());

            await writer.Complete();

            var observations = await writer.DataSet.ListObservations([], [], null);

            Assert.Equal(
                _testDataSet.Observations.Select(observation => observation.Id).Order(),
                observations.Select(observation => observation.Id).Order()
            );
            Assert.Equal(
                _testDataSet.Locations.Select(location => location.GeographicLevel).Order(),
                (await writer.ListGeographicLevels()).Order()
            );
        }
    }

    private ParquetV1DataSetWriter BuildWriter()
    {
        return new ParquetV1DataSetWriter(
            _testDataSet.SubjectId,
            BuildParquetV1DataSetFactory(_testDataSet.PathResolver),
            _testDataSet.BuildWriter(),
            new SequentialGuidGenerator()
        );
    }
}
