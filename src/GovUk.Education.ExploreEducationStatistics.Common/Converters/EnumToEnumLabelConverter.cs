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
            value => FromProvider(value),
            mappingHints)
    {
    }

    private static string ToProvider(TEnum value)
    {
        return value.GetEnumValue();
    }

    private static TEnum FromProvider(string value)
    {
        if (value == null)
        {
            throw new ArgumentOutOfRangeException($"Enum label cannot be null");
        }
        
        if (Lookup.TryGetValue(value.ToLower(), out var enumLabel))
        {
            return enumLabel;
        }

        throw new ArgumentOutOfRangeException($"No enum label found for {value}");
    }
}
