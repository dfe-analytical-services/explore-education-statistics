using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions
{
    public static class EnumExtensions
    {
        public static string GetEnumLabel(this Enum enumValue)
        {
            return GetEnumLabelValueAttribute(enumValue)?.Label ?? enumValue.ToString();
        }

        public static string GetEnumValue(this Enum enumValue)
        {
            return GetEnumLabelValueAttribute(enumValue)?.Value ?? enumValue.ToString();
        }

        public static TAttribute GetEnumAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());

            return memberInfo[0].GetCustomAttributes(typeof(TAttribute), false)
                .OfType<TAttribute>()
                .FirstOrDefault();
        }

        private static EnumLabelValueAttribute GetEnumLabelValueAttribute(this Enum enumValue)
        {
            return enumValue.GetEnumAttribute<EnumLabelValueAttribute>();
        }
    }
}