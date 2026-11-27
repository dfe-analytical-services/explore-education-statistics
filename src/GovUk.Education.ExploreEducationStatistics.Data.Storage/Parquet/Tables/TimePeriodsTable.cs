#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

public static class TimePeriodsTable
{
    public const string TableName = "time_periods";
    public const string ParquetFile = "time_periods.parquet";

    public static class Cols
    {
        public const string Id = "id";

        // @MarkFix Written by ParquetDataSetWriter but never read by ParquetV1DataSet.
        public const string Period = "period";

        // @MarkFix Written by ParquetDataSetWriter but never read by ParquetV1DataSet.
        public const string Identifier = "identifier";
        public const string Year = "ees_year";
        public const string TimeIdentifier = "ees_time_identifier";
    }
}
