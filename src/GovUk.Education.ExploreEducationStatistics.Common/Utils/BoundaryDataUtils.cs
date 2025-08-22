namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class BoundaryDataUtils
{
    public static string GetCode(IDictionary<string, object> properties)
    {
        var key = GetKeyBySuffix(properties, "CD");

        return properties.TryGetValue(key, out var code)
            ? code.ToString() ?? ""
            : throw new ArgumentException("Required key not found (expects key ending 'CD')");
    }

    public static string GetName(IDictionary<string, object> properties)
    {
        var key = GetKeyBySuffix(properties, "NM");

        return properties.TryGetValue(key, out var name)
            ? name.ToString() ?? ""
            : throw new ArgumentException("Required key not found (expects key ending 'NM')");
    }

    private static string GetKeyBySuffix(IDictionary<string, object> properties, string keySuffix) =>
        properties.Keys.SingleOrDefault(k => k.EndsWith(keySuffix, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
}
