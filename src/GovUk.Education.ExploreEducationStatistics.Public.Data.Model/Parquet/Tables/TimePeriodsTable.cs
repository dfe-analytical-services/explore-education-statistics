namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;

public static class TimePeriodsTable
{
    public const string TableName = "time_periods";
    public const string ParquetFile = "time_periods.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string Period = "period";
        public const string Identifier = "identifier";
    }

    private static readonly TableRef DefaultRef = new(TableName);

    public static TableRef Ref(string? table = null) => table is not null ? new(table) : DefaultRef;

    public class TableRef(string table)
    {
        public readonly string Id = $"{table}.{Cols.Id}";
        public readonly string Period = $"{table}.{Cols.Period}";
        public readonly string Identifier = $"{table}.{Cols.Identifier}";
    }
}
