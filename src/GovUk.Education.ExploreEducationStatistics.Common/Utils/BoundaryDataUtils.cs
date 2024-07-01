#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;
public static class BoundaryDataUtils
{
    public static (string Name, string Code) GetFeatureDetails(IDictionary<string, object> properties)
    {
        // OBJECTID field appears to start being excluded from boundary files some time during 2022
        var propertiesContainsObjectId = properties
            .ElementAt(0)
            .Key
            .ToString()
            .Equals("OBJECTID", StringComparison.InvariantCultureIgnoreCase);

        var nameIndex = propertiesContainsObjectId ? 2 : 1;
        var codeIndex = propertiesContainsObjectId ? 1 : 0;

        var nameKey = properties.ElementAt(nameIndex).Key;
        var codeKey = properties.ElementAt(codeIndex).Key;

        if (!nameKey.EndsWith("NM", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{nameKey} does not match expected format (expects key ending \"NM\")");
        }

        if (!codeKey.EndsWith("CD", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"{codeKey} does not match expected format (expects key ending \"CD\")");
        }

        var nameExists = properties.TryGetValue(nameKey, out var name);
        var codeExists = properties.TryGetValue(codeKey, out var code);

        return nameExists && codeExists
            ? (Name: name.ToString(), Code: code.ToString())
            : throw new ArgumentException("Required pair of keys not found (expects keys ending \"NM\" and \"CD\")");
    }
}
