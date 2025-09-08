using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;

public static class DataTable
{
    public const string TableName = "data";
    public const string ParquetFile = "data.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string GeographicLevel = "geographic_level";
        public const string TimePeriodId = "time_period_id";

        public static string LocationId(GeographicLevel geographicLevel) =>
            $"locations_{geographicLevel.GetEnumValue().ToLower()}_id";

        public static string Filter(FilterMeta filter) => $"\"{filter.Column}\"";

        public static string Indicator(IndicatorMeta indicator) => $"\"{indicator.Column}\"";
    }

    private static readonly TableRef DefaultRef = new(TableName);

    public static TableRef Ref(string? table = null) => table is not null ? new(table) : DefaultRef;

    public class TableRef(string table)
    {
        public readonly string Id = $"{table}.{Cols.Id}";
        public readonly string GeographicLevel = $"{table}.{Cols.GeographicLevel}";
        public readonly string TimePeriodId = $"{table}.{Cols.TimePeriodId}";

        public string Col(string column) => $"{table}.\"{column}\"";

        public string LocationId(GeographicLevel level) => $"{table}.{Cols.LocationId(level)}";
    }
}
