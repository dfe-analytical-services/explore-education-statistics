namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;

public static class FiltersTable
{
    public const string TableName = "filters";

    public static class Cols
    {
        public const string Id = "id";
        public const string PublicId = "public_id";
        public const string Label = "label";
        public const string ColumnName = "column_name";
    }

    public static string Alias(FilterMeta filter) => $"\"{filter.PublicId}\"";

    private static readonly TableRef DefaultRef = new(TableName);

    public static TableRef Ref(FilterMeta? filter = null)
        => filter is not null ? new(Alias(filter)) : DefaultRef;

    public class TableRef(string table)
    {
        public readonly string Id = $"{table}.{Cols.Id}";
        public readonly string PublicId = $"{table}.{Cols.PublicId}";
        public readonly string Label = $"{table}.{Cols.Label}";
        public readonly string ColumnName = $"{table}.{Cols.ColumnName}";

        public string Col(string column) => $"{table}.\"{column}\"";
    }
}
