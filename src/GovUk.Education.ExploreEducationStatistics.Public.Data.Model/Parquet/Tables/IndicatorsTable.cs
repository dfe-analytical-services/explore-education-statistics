namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;

public static class IndicatorsTable
{
    public const string TableName = "indicators";
    public const string ParquetFile = "indicators.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string Column = "\"column\"";
        public const string Label = "label";
        public const string Unit = "unit";
        public const string DecimalPlaces = "decimal_places";
    }

    public static string Alias(IndicatorMeta indicator) => $"\"{indicator.Column}\"";

    private static readonly TableRef DefaultRef = new(TableName);

    public static TableRef Ref(IndicatorMeta? indicator = null) =>
        indicator is not null ? new(Alias(indicator)) : DefaultRef;

    public class TableRef(string table)
    {
        public readonly string Id = $"{table}.{Cols.Id}";
        public readonly string Column = $"{table}.{Cols.Column}";
        public readonly string Label = $"{table}.{Cols.Label}";
        public readonly string Unit = $"{table}.{Cols.Unit}";
        public readonly string DecimalPlaces = $"{table}.{Cols.DecimalPlaces}";

        public string Col(string column) => $"{table}.\"{column}\"";
    }
}
