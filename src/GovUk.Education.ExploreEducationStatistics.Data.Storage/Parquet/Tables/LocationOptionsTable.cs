#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

// @MarkFix The whole table is written by ParquetDataSetWriter but never read by ParquetV1DataSet, which reads
// LocationsTable instead.
public static class LocationOptionsTable
{
    public const string TableName = "location_options";
    public const string ParquetFile = "location_options.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string Label = "label";
        public const string Level = "level";
        public const string Code = "code";
        public const string OldCode = "old_code";
        public const string Urn = "urn";
        public const string LaEstab = "laestab";
        public const string Ukprn = "ukprn";
    }
}
