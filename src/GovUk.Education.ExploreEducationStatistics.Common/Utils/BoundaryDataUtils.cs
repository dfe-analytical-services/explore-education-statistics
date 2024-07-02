#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;
public static class BoundaryDataUtils
{
    public static string? GetCode(IDictionary<string, object> properties)
    {
        var key = GetKeyBySuffix(properties, "CD");
        var keyExists = properties.TryGetValue(key, out var code);

        return keyExists
            ? code?.ToString()
            : throw new ArgumentException("Required key not found (expects key ending \"CD\")");
    }

    public static string? GetName(IDictionary<string, object> properties)
    {
        var key = GetKeyBySuffix(properties, "NM");
        var keyExists = properties.TryGetValue(key, out var name);

        return keyExists
            ? name?.ToString()
            : throw new ArgumentException("Required key not found (expects key ending \"NM\")");
    }

    private static string GetKeyBySuffix(
        IDictionary<string, object> properties,
        string keySuffix)
            => properties.Keys.Single(k => k.EndsWith(keySuffix));
}
