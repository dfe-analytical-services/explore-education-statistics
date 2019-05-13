using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public class EnumToEnumValueConverter<TEnum> : ValueConverter<TEnum, string>
    {
        public EnumToEnumValueConverter(ConverterMappingHints mappingHints = null) :
            base(x => ToProvider(x),
                x => FromProvider(x),
                mappingHints)
        {
        }

        private static string ToProvider(TEnum value)
        {
            var enumType = value.GetType();
            var enumTypeMemberInfo = enumType.GetMember(value.ToString());
            var enumLabelAttribute = (EnumLabelValueAttribute) enumTypeMemberInfo[0]
                .GetCustomAttributes(typeof(EnumLabelValueAttribute), false)
                .FirstOrDefault();
            return enumLabelAttribute.Value;
        }

        private static TEnum FromProvider(string value)
        {
            var enumType = typeof(TEnum);
            foreach (TEnum val in Enum.GetValues(enumType))
            {
                var enumTypeMemberInfo = enumType.GetMember(val.ToString());
                var enumLabelAttribute = (EnumLabelValueAttribute) enumTypeMemberInfo[0]
                    .GetCustomAttributes(typeof(EnumLabelValueAttribute), false)
                    .FirstOrDefault();

                if (enumLabelAttribute.Value == value)
                {
                    return val;
                }
            }

            throw new ArgumentException("The value '" + value + "' is not supported.");
        }
    }
}