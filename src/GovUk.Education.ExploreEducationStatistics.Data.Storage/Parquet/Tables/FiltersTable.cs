#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

/// <summary>
/// Filter level meta which the Public API holds in its database rather than in parquet files.
/// </summary>
public static class FiltersTable
{
    public const string TableName = "filters";
    public const string ParquetFile = "filters.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string PublicId = "public_id";
        public const string Column = "\"column\"";
        public const string Label = "label";
        public const string Hint = "hint";
        public const string GroupCsvColumn = "group_csv_column";
        public const string ParentFilter = "parent_filter";
        public const string AutoSelectFilterItemId = "auto_select_filter_item_id";
        public const string AutoSelectFilterItemLabel = "auto_select_filter_item_label";
    }
}
