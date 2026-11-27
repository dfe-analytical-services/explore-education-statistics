#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

public static class DataTable
{
    public const string TableName = "data";
    public const string ParquetFile = "data.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string GeographicLevel = "geographic_level";
        public const string TimePeriodId = "time_period_id";
        public const string ObservationId = "ees_observation_id";
        public const string LocationId = "ees_location_id";
        public const string CsvRow = "ees_csv_row";

        // @MarkFix Written by ParquetDataSetWriter but never read by ParquetV1DataSet.
        public static string LocationOptionId(GeographicLevel geographicLevel) =>
            $"locations_{geographicLevel.GetEnumValue().ToLower()}_id";
    }

    public static bool IsReservedColumn(string column) =>
        column.ToLowerInvariant()
            is Cols.Id
                or Cols.GeographicLevel
                or Cols.TimePeriodId
                or Cols.ObservationId
                or Cols.LocationId
                or Cols.CsvRow
        || EnumUtil.GetEnums<GeographicLevel>().Any(level => Cols.LocationOptionId(level) == column.ToLowerInvariant());
}
