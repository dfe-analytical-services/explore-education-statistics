using System;
using System.Collections.Generic;
using System.Globalization;
using static System.Globalization.CultureInfo;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    /**
     * Compares two strings which may be integer or DateTime values represented as strings.
     * If both values can be converted to integer then they are compared as integer values.
     * If both values can be converted to DateTime then they are compared as DateTime values.
     * If conversion is not possible then they are compared as strings.  
     */
    public class LabelRelationalComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;

            if (int.TryParse(x, out var xIntValue) && int.TryParse(y, out var yIntValue))
            {
                return xIntValue.CompareTo(yIntValue);
            }

            const string dateFormat = "dd/MM/yyyy";
            const DateTimeStyles style = DateTimeStyles.None;
            if (DateTime.TryParseExact(x, dateFormat, InvariantCulture, style, out var xDateTimeValue)
                && DateTime.TryParseExact(y, dateFormat, InvariantCulture, style, out var yDateTimeValue))
            {
                return xDateTimeValue.CompareTo(yDateTimeValue);
            }

            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }
}