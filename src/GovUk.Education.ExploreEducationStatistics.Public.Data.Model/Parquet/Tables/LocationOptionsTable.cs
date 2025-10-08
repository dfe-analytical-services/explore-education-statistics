using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet.Tables;

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
        public const string Ukprn = "ukprn";
        public const string Urn = "urn";
        public const string LaEstab = "laestab";
    }

    public static string Alias(GeographicLevel geographicLevel) =>
        $"locations_{geographicLevel.GetEnumValue().ToLower()}";

    private static TableRef DefaultRef => new(TableName);

    public static TableRef Ref(GeographicLevel? level = null) => level.HasValue ? new(Alias(level.Value)) : DefaultRef;

    public class TableRef(string table)
    {
        public readonly string Id = $"{table}.{Cols.Id}";
        public readonly string Label = $"{table}.{Cols.Label}";
        public readonly string Level = $"{table}.{Cols.Level}";
        public readonly string PublicId = $"{table}.{Cols.PublicId}";
        public readonly string Code = $"{table}.{Cols.Code}";
        public readonly string OldCode = $"{table}.{Cols.OldCode}";
        public readonly string Ukprn = $"{table}.{Cols.Ukprn}";
        public readonly string Urn = $"{table}.{Cols.Urn}";
        public readonly string LaEstab = $"{table}.{Cols.LaEstab}";

        public string Col(string column) => $"{table}.\"{column}\"";
    }
}
