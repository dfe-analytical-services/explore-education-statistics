#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

/// <summary>
/// Mirrors the Public API's location options table, with one row per distinct location at each geographic level.
/// </summary>
public static class LocationOptionsTable
{
    public const string TableName = "location_options";
    public const string ParquetFile = "location_options.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string Label = "label";
        public const string Level = "level";
        public const string PublicId = "public_id";
        public const string Code = "code";
        public const string OldCode = "old_code";
        public const string Urn = "urn";
        public const string LaEstab = "laestab";
        public const string Ukprn = "ukprn";
    }
}
