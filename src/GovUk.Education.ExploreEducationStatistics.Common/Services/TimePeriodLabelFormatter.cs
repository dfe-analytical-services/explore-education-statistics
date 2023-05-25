using System;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static System.String;
using static GovUk.Education.ExploreEducationStatistics.Common.Database.TimePeriodYearFormat;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class TimePeriodLabelFormatter
    {
        private static readonly Regex YearRegex = new Regex(@"^([0-9]{4})?$");
        private TimePeriodLabelFormat LabelFormat { get; }
        private TimePeriodYearFormat YearFormat { get; }

        private TimePeriodLabelFormatter(TimePeriodLabelFormat labelFormat,
            TimePeriodYearFormat yearFormat)
        {
            LabelFormat = labelFormat;
            YearFormat = yearFormat;
        }

        public static string Format(
            int year,
            TimeIdentifier timeIdentifier,
            TimePeriodLabelFormat? overridingLabelFormat = null)
        {
            return FormatterFor(timeIdentifier, overridingLabelFormat).FormatInternal(year, timeIdentifier);
        }

        public static string FormatYear(int year, TimeIdentifier timeIdentifier)
        {
            return FormatterFor(timeIdentifier).FormatYearInternal(year);
        }

        public static string FormatCsvYear(int year, TimeIdentifier timeIdentifier)
        {
            return FormatterFor(timeIdentifier).FormatCsvYearInternal(year);
        }

        private string FormatInternal(int year, TimeIdentifier timeIdentifier)
        {
            var labelValueAttribute = timeIdentifier.GetEnumAttribute<TimeIdentifierMetaAttribute>();
            var formattedYear = FormatYearInternal(year);
            return LabelFormat switch
            {
                TimePeriodLabelFormat.FullLabel => $"{formattedYear} {labelValueAttribute.Label}",
                TimePeriodLabelFormat.FullLabelBeforeYear => $"{labelValueAttribute.Label} {formattedYear}",
                TimePeriodLabelFormat.NoLabel => formattedYear,
                TimePeriodLabelFormat.ShortLabel => IsNullOrEmpty(labelValueAttribute.ShortLabel)
                    ? formattedYear
                    : $"{formattedYear} {labelValueAttribute.ShortLabel}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private string FormatYearInternal(int year)
        {
            if (YearRegex.Match(year.ToString()).Success)
            {
                return YearFormat switch
                {
                    Default => year.ToString(),
                    Academic => $"{year}/{(year + 1) % 100:D2}", // Only want the last two digits,
                    Fiscal => $"{year}-{(year + 1) % 100:D2}", // Only want the last two digits,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            throw new ArgumentOutOfRangeException();
        }

        private string FormatCsvYearInternal(int year)
        {
            if (YearRegex.Match(year.ToString()).Success)
            {
                return YearFormat switch
                {
                    Academic or Fiscal => $"{year}{(year + 1) % 100:D2}",
                    _ => year.ToString()
                };
            }

            throw new ArgumentOutOfRangeException();
        }

        private static TimePeriodLabelFormatter FormatterFor(
            TimeIdentifier timeIdentifier,
            TimePeriodLabelFormat? overridingLabelFormat = null)
        {
            var labelValueAttribute = timeIdentifier.GetEnumAttribute<TimeIdentifierMetaAttribute>();
            return overridingLabelFormat.HasValue
                ? new TimePeriodLabelFormatter(overridingLabelFormat.Value, labelValueAttribute.YearFormat)
                : new TimePeriodLabelFormatter(labelValueAttribute.LabelFormat,
                    labelValueAttribute.YearFormat);
        }
    }
}
