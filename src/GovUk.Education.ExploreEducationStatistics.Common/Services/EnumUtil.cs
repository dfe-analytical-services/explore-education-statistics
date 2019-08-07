using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public static class EnumUtil
    {
        public static TEnum GetFromString<TEnum>(string value) where TEnum : Enum
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