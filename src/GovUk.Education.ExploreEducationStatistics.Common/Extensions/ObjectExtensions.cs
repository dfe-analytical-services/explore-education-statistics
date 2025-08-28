#nullable enable
using System.Reflection;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class ObjectExtensions
{
    public static Dictionary<string, object?> ToDictionary(
        this object obj,
        BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public)
    {
        var properties = obj.GetType().GetProperties(bindingFlags);

        return properties.ToDictionary(
            property => property.Name,
            property => property.GetValue(obj)
        );
    }
}
