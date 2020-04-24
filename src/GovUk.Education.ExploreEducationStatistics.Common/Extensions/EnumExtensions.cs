using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Services;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions
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
        
        
        public static List<EnumValue> GetValues<T>() where T: Enum
        {
            return (from object itemType in Enum.GetValues(typeof(T)) select new EnumValue() {Name = Enum.GetName(typeof(T), itemType), Value = (int) itemType}).ToList();
        }
        
        public class EnumValue
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}