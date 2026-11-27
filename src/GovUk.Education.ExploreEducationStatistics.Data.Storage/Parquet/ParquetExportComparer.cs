#nullable enable
using System.Security.Cryptography;
using System.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;

/// <summary>
/// Compares a data set exported from StatsDB to parquet files with the StatsDB data set it was exported from.
/// Remove this along with the export once every StatsDB data set has been migrated.
/// </summary>
public static class ParquetExportComparer
{
    private const int MaxReportedDifferences = 100;
    private const int ObservationBatchSize = 10_000;

    public static async Task<ParquetExportComparison> Compare(
        IStorageDataSet statsDbDataSet,
        IStorageDataSet parquetDataSet,
        CancellationToken cancellationToken = default
    )
    {
        var differences = new Differences();

        var statsDbFilters = await statsDbDataSet.ListFilters(includeItems: true, cancellationToken);
        var parquetFilters = await parquetDataSet.ListFilters(includeItems: true, cancellationToken);

        CompareById(
            "Filter",
            statsDbFilters,
            parquetFilters,
            filter => filter.Id,
            filter => new
            {
                filter.Label,
                filter.Name,
                filter.Hint,
                filter.GroupCsvColumn,
                filter.ParentFilter,
                filter.AutoSelectFilterItemId,
                filter.AutoSelectFilterItemLabel,
            },
            differences
        );

        var statsDbFilterGroups = statsDbFilters.SelectMany(filter => filter.FilterGroups).ToList();
        var parquetFilterGroups = parquetFilters.SelectMany(filter => filter.FilterGroups).ToList();

        CompareById(
            "Filter group",
            statsDbFilterGroups,
            parquetFilterGroups,
            group => group.Id,
            group => new { group.FilterId, group.Label },
            differences
        );

        CompareById(
            "Filter item",
            statsDbFilterGroups.SelectMany(group => group.FilterItems),
            parquetFilterGroups.SelectMany(group => group.FilterItems),
            item => item.Id,
            item => new { item.FilterGroupId, item.Label },
            differences
        );

        var statsDbIndicatorGroups = await statsDbDataSet.ListIndicatorGroups(cancellationToken);
        var parquetIndicatorGroups = await parquetDataSet.ListIndicatorGroups(cancellationToken);

        CompareById(
            "Indicator group",
            statsDbIndicatorGroups,
            parquetIndicatorGroups,
            group => group.Id,
            group => new { group.Label },
            differences
        );

        CompareById(
            "Indicator",
            statsDbIndicatorGroups.SelectMany(group => group.Indicators),
            parquetIndicatorGroups.SelectMany(group => group.Indicators),
            indicator => indicator.Id,
            indicator => new
            {
                indicator.IndicatorGroupId,
                indicator.Label,
                indicator.Name,
                indicator.Unit,
                indicator.DecimalPlaces,
            },
            differences
        );

        CompareById(
            "Location",
            await statsDbDataSet.ListLocations(cancellationToken: cancellationToken),
            await parquetDataSet.ListLocations(cancellationToken: cancellationToken),
            location => location.Id,
            DescribeLocation,
            differences
        );

        var statsDbTimePeriods = await statsDbDataSet.ListTimePeriods(cancellationToken: cancellationToken);
        var parquetTimePeriods = await parquetDataSet.ListTimePeriods(cancellationToken: cancellationToken);

        if (!statsDbTimePeriods.SequenceEqual(parquetTimePeriods))
        {
            differences.Add(
                $"Time periods differ. StatsDB: [{DescribeTimePeriods(statsDbTimePeriods)}]. "
                    + $"Parquet: [{DescribeTimePeriods(parquetTimePeriods)}]"
            );
        }

        var (statsDbObservationCount, parquetObservationCount) = await CompareObservations(
            statsDbDataSet,
            parquetDataSet,
            differences,
            cancellationToken
        );

        return new ParquetExportComparison(
            StatsDbObservationCount: statsDbObservationCount,
            ParquetObservationCount: parquetObservationCount,
            DifferenceCount: differences.Count,
            Differences: differences.Reported
        );
    }

    private static async Task<(long StatsDbCount, long ParquetCount)> CompareObservations(
        IStorageDataSet statsDbDataSet,
        IStorageDataSet parquetDataSet,
        Differences differences,
        CancellationToken cancellationToken
    )
    {
        var statsDbHashes = new Dictionary<Guid, ulong>();

        await foreach (
            var batch in statsDbDataSet.ListObservationBatches(
                filterItemIds: [],
                locationIds: [],
                timePeriod: null,
                ObservationBatchSize,
                cancellationToken
            )
        )
        {
            foreach (var observation in batch)
            {
                statsDbHashes[observation.Id] = Hash(DescribeObservation(observation));
            }
        }

        long statsDbCount = statsDbHashes.Count;
        long parquetCount = 0;

        await foreach (
            var batch in parquetDataSet.ListObservationBatches(
                filterItemIds: [],
                locationIds: [],
                timePeriod: null,
                ObservationBatchSize,
                cancellationToken
            )
        )
        {
            foreach (var observation in batch)
            {
                parquetCount++;

                var description = DescribeObservation(observation);

                if (!statsDbHashes.Remove(observation.Id, out var statsDbHash))
                {
                    differences.Add($"Observation {observation.Id} is missing from StatsDB. Parquet: {description}");
                }
                else if (statsDbHash != Hash(description))
                {
                    differences.Add($"Observation {observation.Id} differs. Parquet: {description}");
                }
            }
        }

        foreach (var observationId in statsDbHashes.Keys)
        {
            differences.Add($"Observation {observationId} is missing from parquet");
        }

        return (statsDbCount, parquetCount);
    }

    private static void CompareById<T>(
        string kind,
        IEnumerable<T> statsDbItems,
        IEnumerable<T> parquetItems,
        Func<T, Guid> getId,
        Func<T, object> describe,
        Differences differences
    )
    {
        var statsDbById = statsDbItems.ToDictionary(getId);
        var parquetById = parquetItems.ToDictionary(getId);

        foreach (var (id, statsDbItem) in statsDbById)
        {
            if (!parquetById.TryGetValue(id, out var parquetItem))
            {
                differences.Add($"{kind} {id} is missing from parquet");
                continue;
            }

            var statsDbDescription = describe(statsDbItem);
            var parquetDescription = describe(parquetItem);

            if (!statsDbDescription.Equals(parquetDescription))
            {
                differences.Add($"{kind} {id} differs. StatsDB: {statsDbDescription}. Parquet: {parquetDescription}");
            }
        }

        foreach (var id in parquetById.Keys.Except(statsDbById.Keys))
        {
            differences.Add($"{kind} {id} is missing from StatsDB");
        }
    }

    private static string DescribeLocation(Location location) =>
        string.Join(
            ", ",
            LocationsTable
                .AttributeColumns.Select(column => (column.Name, Value: column.GetValue(location)))
                .Where(column => column.Value is not null)
                .Select(column => $"{column.Name}={column.Value}")
                .Prepend($"{LocationsTable.Cols.GeographicLevel}={location.GeographicLevel}")
        );

    private static string DescribeTimePeriods(List<(int Year, TimeIdentifier TimeIdentifier)> timePeriods) =>
        string.Join(", ", timePeriods.Select(timePeriod => $"{timePeriod.Year} {timePeriod.TimeIdentifier}"));

    private static string DescribeObservation(Observation observation)
    {
        var filterItemIds = observation.FilterItems.Select(filterItem => filterItem.FilterItemId).Order();

        var measures = observation
            .Measures.Where(measure => measure.Value is not null)
            .OrderBy(measure => measure.Key)
            .Select(measure => $"{measure.Key}={measure.Value}");

        return $"Location: {observation.LocationId}, "
            + $"Time period: {observation.Year} {observation.TimeIdentifier}, "
            + $"CSV row: {observation.CsvRow}, "
            + $"Filter items: [{string.Join(", ", filterItemIds)}], "
            + $"Measures: [{string.Join(", ", measures)}]";
    }

    private static ulong Hash(string description) =>
        BitConverter.ToUInt64(SHA256.HashData(Encoding.UTF8.GetBytes(description)));

    private class Differences
    {
        public int Count { get; private set; }

        public List<string> Reported { get; } = [];

        public void Add(string difference)
        {
            Count++;

            if (Reported.Count < MaxReportedDifferences)
            {
                Reported.Add(difference);
            }
        }
    }
}

public record ParquetExportComparison(
    long StatsDbObservationCount,
    long ParquetObservationCount,
    int DifferenceCount,
    List<string> Differences
)
{
    public bool Matches => DifferenceCount == 0;
}
