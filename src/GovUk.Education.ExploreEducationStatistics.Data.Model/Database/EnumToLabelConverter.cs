using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public class EnumToLabelConverter<TEnum> : ValueConverter<TEnum, string>
    {
        public EnumToLabelConverter(ConverterMappingHints mappingHints = null) :
            base(x => ToProvider(x),
                x => FromProvider(x),
                mappingHints)
        {
        }

        private static string ToProvider(TEnum value)
        {
            var enumType = value.GetType();
            var enumTypeMemberInfo = enumType.GetMember(value.ToString());
            var enumLabelAttribute = (EnumLabelAttribute) enumTypeMemberInfo[0]
                .GetCustomAttributes(typeof(EnumLabelAttribute), false)
                .FirstOrDefault();
            return enumLabelAttribute.Label;
        }

        private static TEnum FromProvider(string value)
        {
            var enumType = typeof(TEnum);
            foreach (TEnum val in Enum.GetValues(enumType))
            {
                var enumTypeMemberInfo = enumType.GetMember(value);
                var enumLabelAttribute = (EnumLabelAttribute) enumTypeMemberInfo[0]
                    .GetCustomAttributes(typeof(EnumLabelAttribute), false)
                    .FirstOrDefault();

                if (enumLabelAttribute.Label == value)
                {
                    return val;
                }
            }

            throw new ArgumentException("The value '" + value + "' is not supported.");
        }
    }
}