namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;

public static class FilterOptionsTable
{
    public const string TableName = "filter_options";
    public const string ParquetFile = "filter_options.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string PublicId = "public_id";
        public const string Label = "label";
        public const string FilterColumn = "filter_column";
        public const string FilterId = "filter_id";
    }

    public static string Alias(FilterMeta filter) => $"\"{filter.Column}\"";

    private static readonly TableRef DefaultRef = new(TableName);

    public static TableRef Ref(FilterMeta? filter = null) =>
        filter is not null ? new(Alias(filter)) : DefaultRef;

    public class TableRef(string table)
    {
        public readonly string Id = $"{table}.{Cols.Id}";
        public readonly string PublicId = $"{table}.{Cols.PublicId}";
        public readonly string Label = $"{table}.{Cols.Label}";
        public readonly string FilterId = $"{table}.{Cols.FilterId}";
        public readonly string FilterColumn = $"{table}.{Cols.FilterColumn}";

        public string Col(string column) => $"{table}.\"{column}\"";
    }
}
