#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

public static class IndicatorsTable
{
    public const string TableName = "indicators";
    public const string ParquetFile = "indicators.parquet";

    public static class Cols
    {
        public const string Column = "\"column\"";
        public const string Label = "label";
        public const string Unit = "unit";
        public const string DecimalPlaces = "decimal_places";
        public const string IndicatorId = "ees_indicator_id";
        public const string IndicatorGroupId = "ees_indicator_group_id";
        public const string IndicatorGroupLabel = "ees_indicator_group_label";
    }
}
