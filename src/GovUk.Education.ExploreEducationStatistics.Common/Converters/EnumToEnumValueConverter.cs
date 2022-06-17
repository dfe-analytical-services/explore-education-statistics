#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters;

public class EnumToEnumValueConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : Enum
{
    private static readonly Dictionary<string, TEnum> Lookup =
        EnumUtil.GetEnumValues<TEnum>().ToDictionary(value => value.GetEnumValue());

    public EnumToEnumValueConverter(
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
        if (Lookup.TryGetValue(value, out var enumVal))
        {
            return enumVal;
        }

        throw new ArgumentOutOfRangeException($"No enum value found for {value}");
    }
}
