#nullable enable
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters;

public class EnumToEnumValueListConverter<TEnum>(ConverterMappingHints? mappingHints = null)
    : ValueConverter<List<TEnum>, List<string>>(
        value => ToProvider(value),
        value => FromProvider(value),
        mappingHints
    )
    where TEnum : Enum
{
    public static List<string> ToProvider(IList<TEnum> values)
    {
        return values
            .Select(EnumToEnumValueConverter<TEnum>.ToProvider)
            .ToList();
    }

    public static List<TEnum> FromProvider(IList<string> values)
    {
        return values
            .Select(EnumToEnumValueConverter<TEnum>.FromProvider)
            .ToList();
    }
}
