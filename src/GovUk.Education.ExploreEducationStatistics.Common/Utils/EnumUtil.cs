#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class EnumUtil
{
    private static class EnumCache<TEnum> where TEnum : Enum
    {
        public static readonly Lazy<Dictionary<string, TEnum>> Values = new(() =>
            GetEnumValues<TEnum>().ToDictionary(e => e.GetEnumValue()));

        public static readonly Lazy<Dictionary<string, TEnum>> Labels = new(() =>
            GetEnumValues<TEnum>().ToDictionary(e => e.GetEnumLabel()));
    }

    public static TEnum GetFromEnumValue<TEnum>(string value) where TEnum : Enum
    {
        if (EnumCache<TEnum>.Values.Value.TryGetValue(value, out var enumValue))
        {
            return enumValue;
        }

        throw new ArgumentException($"The value '{value}' is not a valid {typeof(TEnum).Name}");
    }

    public static TEnum GetFromEnumLabel<TEnum>(string label) where TEnum : Enum
    {
        if (EnumCache<TEnum>.Labels.Value.TryGetValue(label, out var enumValue))
        {
            return enumValue;
        }

        throw new ArgumentException($"The label '{label}' is not a valid {typeof(TEnum).Name}");
    }

    public static List<TEnum> GetEnumValues<TEnum>()
    {
        return Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
    }

    public static TEnum[] GetEnumValuesAsArray<TEnum>()
    {
        return GetEnumValues<TEnum>().ToArray();
    }
}
