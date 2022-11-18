#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters;

public class EnumToEnumLabelConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : Enum
{
    private static readonly Dictionary<string, TEnum> Lookup =
        EnumUtil.GetEnumValues<TEnum>().ToDictionary(value => value.GetEnumLabel().ToLower());

    public EnumToEnumLabelConverter(
        ConverterMappingHints? mappingHints = null) :
        base(value => ToProvider(value),
            label => FromProvider(label),
            mappingHints)
    {
    }

    private static string ToProvider(TEnum value)
    {
        return value.GetEnumLabel();
    }

    private static TEnum FromProvider(string label)
    {
        if (label == null)
        {
            throw new ArgumentOutOfRangeException($"{nameof(label)} cannot be null");
        }
        
        if (Lookup.TryGetValue(label.ToLower(), out var enumValue))
        {
            return enumValue;
        }

        throw new ArgumentOutOfRangeException($"No enum value found for {nameof(label)} {label}");
    }
}
