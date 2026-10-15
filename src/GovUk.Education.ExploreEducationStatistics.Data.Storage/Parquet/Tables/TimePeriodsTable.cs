#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

/// <summary>
/// Mirrors the Public API's time periods table.
/// </summary>
public static class TimePeriodsTable
{
    public const string TableName = "time_periods";
    public const string ParquetFile = "time_periods.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string Period = "period";
        public const string Identifier = "identifier";
        public const string Year = "ees_year";
        public const string TimeIdentifier = "ees_time_identifier";
    }
}
