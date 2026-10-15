#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet;

public static class ParquetSqlUtils
{
    /// <summary>
    /// Quotes an identifier such as a column name taken from a data set's CSV file.
    /// </summary>
    public static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";

    /// <summary>
    /// Creates a string literal, e.g. for a file path, where a parameter can't be used.
    /// </summary>
    public static string Literal(string value) => $"'{value.Replace("'", "''")}'";
}
