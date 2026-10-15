#nullable enable
using System.Reflection;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.Parquet.Tables;

/// <summary>
/// The full StatsDB location rows used by a data set, which the Public API's location options can't represent.
/// </summary>
public static partial class LocationsTable
{
    public const string TableName = "locations";
    public const string ParquetFile = "locations.parquet";

    public static class Cols
    {
        public const string Id = "id";
        public const string GeographicLevel = "geographic_level";
    }

    /// <summary>
    /// One column per <see cref="Location" /> attribute property, e.g. <c>LocalAuthority_OldCode</c> is stored
    /// as <c>local_authority_old_code</c>.
    /// </summary>
    public static readonly IReadOnlyList<AttributeColumn> AttributeColumns = typeof(Location)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(property => property.PropertyType == typeof(string) && property.CanWrite)
        .Select(property => new AttributeColumn(ToColumnName(property.Name), property))
        .ToList();

    public record AttributeColumn(string Name, PropertyInfo Property)
    {
        public string? GetValue(Location location) => (string?)Property.GetValue(location);

        public void SetValue(Location location, string? value) => Property.SetValue(location, value);
    }

    private static string ToColumnName(string propertyName) =>
        string.Join('_', propertyName.Split('_').Select(part => PascalCaseBoundary().Replace(part, "_$1").ToLower()));

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex PascalCaseBoundary();
}
