#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

/// <summary>
/// Mirrors the Public API's filter options table, with one row per StatsDB filter item.
/// </summary>
public static class FilterOptionsTable
{
    public const string TableName = "filter_options";
    public const string ParquetFile = "filter_options.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string Label = "label";
        public const string PublicId = "public_id";
        public const string FilterId = "filter_id";
        public const string FilterColumn = "filter_column";
        public const string FilterItemId = "ees_filter_item_id";
        public const string FilterGroupId = "ees_filter_group_id";
        public const string FilterGroupLabel = "ees_filter_group_label";
        public const string EesFilterId = "ees_filter_id";
    }
}
