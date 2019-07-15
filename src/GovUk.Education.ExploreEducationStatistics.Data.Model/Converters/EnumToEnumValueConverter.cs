using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Converters
{
    public class EnumToEnumValueConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : Enum
    {
        public EnumToEnumValueConverter(ConverterMappingHints mappingHints = null) :
            base(x => ToProvider(x),
                x => FromProvider(x),
                mappingHints)
        {
        }

        private static string ToProvider(TEnum value)
        {
            return ((Enum) value).GetEnumValue();
        }

        private static TEnum FromProvider(string value)
        {
            foreach (var val in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                if (val.GetEnumValue().Equals(value))
                {
                    return val;
                }
            }

            throw new ArgumentException("The value '" + value + "' is not supported.");
        }
    }
}