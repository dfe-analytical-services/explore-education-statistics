#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;

public static class LocationCsvUtils
{
    private static readonly Lazy<ImmutableArray<string>> AllCsvColumnsLazy = new(
        () =>
        {
            var baseType = typeof(LocationAttribute);

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetLoadableTypes()
                    .Where(type => type != baseType)
                    .Where(baseType.IsAssignableFrom))
                // This is a bit hacky, but we initialise empty attribute objects that we can call
                // the `GetCsvValues` on each one. This means that we don't need to define any more
                // methods for getting the CSV columns on each type (less verbose and error prone).
                .Select(type => (LocationAttribute)FormatterServices.GetUninitializedObject(type))
                .OrderBy(attribute => attribute.CsvPriority)
                .SelectMany(attribute => attribute.CsvValues.Select(kv => kv.Key))
                .Distinct()
                .ToImmutableArray();
        }
    );

    public static ImmutableArray<string> AllCsvColumns() => AllCsvColumnsLazy.Value;

    public static Dictionary<string, string> GetCsvValues(this Location location)
    {
        return location.GetAttributes()
            .SelectMany(attribute => attribute.CsvValues)
            .ToDictionary(kv => kv.Key, kv => kv.Value);
    }
}
