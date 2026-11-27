#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Utils;
using Moq;
using Xunit;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Tests.Parquet;

public class ParquetExportComparerTests : IDisposable
{
    private readonly ParquetTestDataSet _testDataSet = new();

    public void Dispose() => _testDataSet.Dispose();

    [Fact]
    public async Task SameData_Matches()
    {
        var parquetDataSet = await _testDataSet.Write();

        var result = await ParquetExportComparer.Compare(BuildStatsDbDataSet(), parquetDataSet);

        Assert.True(result.Matches);
        Assert.Empty(result.Differences);
        Assert.Equal(6, result.StatsDbObservationCount);
        Assert.Equal(6, result.ParquetObservationCount);
    }

    [Fact]
    public async Task MeasureDiffers_ReportsObservation()
    {
        var parquetDataSet = await _testDataSet.Write();
        var observation = _testDataSet.Observations[0];
        observation.Measures[_testDataSet.Enrolments.Id] = "999";

        var result = await ParquetExportComparer.Compare(BuildStatsDbDataSet(), parquetDataSet);

        Assert.False(result.Matches);
        var difference = Assert.Single(result.Differences);
        Assert.StartsWith($"Observation {observation.Id} differs", difference);
    }

    [Fact]
    public async Task ObservationMissingFromStatsDb_ReportsObservationAndCounts()
    {
        var parquetDataSet = await _testDataSet.Write();
        var observation = _testDataSet.Observations[0];

        var result = await ParquetExportComparer.Compare(
            BuildStatsDbDataSet(_testDataSet.Observations.Skip(1).ToList()),
            parquetDataSet
        );

        var difference = Assert.Single(result.Differences);
        Assert.StartsWith($"Observation {observation.Id} is missing from StatsDB", difference);
        Assert.Equal(5, result.StatsDbObservationCount);
        Assert.Equal(6, result.ParquetObservationCount);
    }

    [Fact]
    public async Task ObservationMissingFromParquet_ReportsObservation()
    {
        var parquetDataSet = await _testDataSet.Write();
        var observation = _testDataSet.Observations[0];
        var extraObservation = new Observation
        {
            Id = Guid.NewGuid(),
            LocationId = observation.LocationId,
            Year = observation.Year,
            TimeIdentifier = observation.TimeIdentifier,
            Measures = observation.Measures,
        };

        var result = await ParquetExportComparer.Compare(
            BuildStatsDbDataSet([.. _testDataSet.Observations, extraObservation]),
            parquetDataSet
        );

        Assert.Equal([$"Observation {extraObservation.Id} is missing from parquet"], result.Differences);
    }

    [Fact]
    public async Task MetaDiffers_ReportsEachDifference()
    {
        var parquetDataSet = await _testDataSet.Write();
        _testDataSet.Year1.Label = "Changed filter item";
        _testDataSet.KeyStage1.Label = "Changed filter group";
        _testDataSet.SchoolType.Hint = "Changed hint";
        _testDataSet.Notes.DecimalPlaces = 1;
        _testDataSet.OtherIndicatorGroup.Label = "Changed indicator group";
        _testDataSet.Newcastle.LocalAuthority_Name = "Changed local authority";

        var result = await ParquetExportComparer.Compare(BuildStatsDbDataSet(), parquetDataSet);

        Assert.Equal(
            [
                $"Filter {_testDataSet.SchoolType.Id} differs",
                $"Filter group {_testDataSet.KeyStage1.Id} differs",
                $"Filter item {_testDataSet.Year1.Id} differs",
                $"Indicator group {_testDataSet.OtherIndicatorGroup.Id} differs",
                $"Indicator {_testDataSet.Notes.Id} differs",
                $"Location {_testDataSet.Newcastle.Id} differs",
            ],
            result.Differences.Select(difference => difference[..difference.IndexOf('.')])
        );
    }

    [Fact]
    public async Task MetaMissing_ReportsWhereItIsMissing()
    {
        var parquetDataSet = await _testDataSet.Write();
        var extraLocation = new Location { Id = Guid.NewGuid() };

        var result = await ParquetExportComparer.Compare(
            BuildStatsDbDataSet(
                filters: [_testDataSet.SchoolType],
                locations: [.. _testDataSet.Locations, extraLocation]
            ),
            parquetDataSet
        );

        Assert.Contains($"Filter {_testDataSet.NcYear.Id} is missing from StatsDB", result.Differences);
        Assert.Contains($"Filter group {_testDataSet.KeyStage1.Id} is missing from StatsDB", result.Differences);
        Assert.Contains($"Filter item {_testDataSet.Year1.Id} is missing from StatsDB", result.Differences);
        Assert.Contains($"Location {extraLocation.Id} is missing from parquet", result.Differences);
    }

    private IStorageDataSet BuildStatsDbDataSet(
        List<Observation>? observations = null,
        List<Filter>? filters = null,
        List<Location>? locations = null
    )
    {
        observations ??= _testDataSet.Observations;

        var dataSet = new Mock<IStorageDataSet>(Strict);

        dataSet
            .Setup(d => d.ListFilters(true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(filters ?? _testDataSet.Filters);
        dataSet
            .Setup(d => d.ListIndicatorGroups(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testDataSet.IndicatorGroups);
        dataSet
            .Setup(d => d.ListLocations(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations ?? _testDataSet.Locations);
        dataSet
            .Setup(d => d.ListTimePeriods(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                TimePeriodQueryUtils.Order(
                    observations.Select(observation => (observation.Year, observation.TimeIdentifier)).Distinct()
                )
            );
        dataSet
            .Setup(d =>
                d.ListObservationBatches(
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<IEnumerable<Guid>>(),
                    null,
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(ToBatches(observations));

        return dataSet.Object;
    }

    private static async IAsyncEnumerable<IReadOnlyList<Observation>> ToBatches(List<Observation> observations)
    {
        await Task.CompletedTask;
        yield return observations;
    }
}
