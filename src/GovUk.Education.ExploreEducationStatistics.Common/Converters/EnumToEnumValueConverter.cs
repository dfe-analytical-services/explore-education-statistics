#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Common.Converters
{
    public class EnumToEnumValueConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : Enum
    {
        public EnumToEnumValueConverter(ConverterMappingHints? mappingHints = null) :
            base(x => ToProvider(x),
                x => FromProvider(x),
                mappingHints)
        {
        }

        private static string ToProvider(TEnum value)
        {
            return value.GetEnumValue();
        }

        private static TEnum FromProvider(string value)
        {
            return EnumUtil.GetFromString<TEnum>(value);
        }
    }
}
