using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Database;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using static System.Int32;
using static System.String;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class TimePeriodLabelFormatter
    {
        
        private static readonly Regex YearRegex = new Regex(@"^([0-9]{4})?$");
        private YearFormatOption YearFormat { get; }
        private TimeIdentifierFormatOption TimeIdentifierFormat { get; }

        private static readonly TimePeriodLabelFormatter Default =
            new TimePeriodLabelFormatter(YearFormatOption.Default, TimeIdentifierFormatOption.Default);

        private static readonly TimePeriodLabelFormatter AcademicOrFiscal =
            new TimePeriodLabelFormatter(YearFormatOption.AcademicOrFiscal, TimeIdentifierFormatOption.Default);

        private static readonly TimePeriodLabelFormatter NoLabel =
            new TimePeriodLabelFormatter(YearFormatOption.Default, TimeIdentifierFormatOption.None);

        private static readonly TimePeriodLabelFormatter AcademicOrFiscalNoLabel =
            new TimePeriodLabelFormatter(YearFormatOption.AcademicOrFiscal, TimeIdentifierFormatOption.None);

        private static readonly TimePeriodLabelFormatter Short =
            new TimePeriodLabelFormatter(YearFormatOption.Default, TimeIdentifierFormatOption.Short);

        private static readonly TimePeriodLabelFormatter AcademicOrFiscalShort =
            new TimePeriodLabelFormatter(YearFormatOption.AcademicOrFiscal, TimeIdentifierFormatOption.Short);

        private static readonly Dictionary<TimePeriodLabelFormat, TimePeriodLabelFormatter> Formatters = new
            Dictionary<TimePeriodLabelFormat, TimePeriodLabelFormatter>
            {
                {
                    TimePeriodLabelFormat.Default, Default
                },
                {
                    TimePeriodLabelFormat.AcademicOrFiscal, AcademicOrFiscal
                },
                {
                    TimePeriodLabelFormat.NoLabel, NoLabel
                },
                {
                    TimePeriodLabelFormat.AcademicOrFiscalNoLabel, AcademicOrFiscalNoLabel
                },
                {
                    TimePeriodLabelFormat.Short, Short
                },
                {
                    TimePeriodLabelFormat.AcademicOrFiscalShort, AcademicOrFiscalShort
                }
            };

        private TimePeriodLabelFormatter(YearFormatOption yearFormat, TimeIdentifierFormatOption timeIdentifierFormat)
        {
            YearFormat = yearFormat;
            TimeIdentifierFormat = timeIdentifierFormat;
        }

        
        public static string Format(int year, TimeIdentifier timeIdentifier)
        {
            return Format(year.ToString(), timeIdentifier);
        }
        
        public static string Format(string year, TimeIdentifier timeIdentifier)
        {
            return FormatterFor(timeIdentifier).FormatInternal(year, timeIdentifier); 
        }
        
        public static string FormatLabel(TimeIdentifier timeIdentifier)
        {
            return FormatterFor(timeIdentifier).FormatLabelInternal(timeIdentifier);
        }
        
        public static string FormatYear(string year, TimeIdentifier timeIdentifier)
        {
            return FormatterFor(timeIdentifier).FormatYearInternal(year);
        }
        
        private string FormatInternal(string year, TimeIdentifier timeIdentifier)
        {
            var formattedLabel = FormatLabelInternal(timeIdentifier);
            var formattedYear = FormatYearInternal(year);
            return IsNullOrEmpty(formattedLabel) ? formattedYear : $"{formattedYear} {formattedLabel}";
        }

        private string FormatYearInternal(string year)
        {
            if (!IsNullOrEmpty(year) && YearRegex.Match(year).Success)
            {
                switch (YearFormat)
                {
                    case YearFormatOption.Default:
                        return year;
                    case YearFormatOption.AcademicOrFiscal:
                        return $"{Parse(year)}/{(Parse(year) % 100) + 1}"; // Only want the last two digits
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return IsNullOrEmpty(year) ? "" : year;
        }
        
        public static TimePeriodLabelFormatter FormatterFor(TimeIdentifier timeIdentifier)
        {
            var labelValueAttribute = timeIdentifier.GetEnumAttribute<TimeIdentifierMetaAttribute>();
            var formatter = Formatters[labelValueAttribute.Format];
            return formatter;
        }

        private string FormatLabelInternal(TimeIdentifier timeIdentifier)
        {
            var labelValueAttribute = timeIdentifier.GetEnumAttribute<TimeIdentifierMetaAttribute>();
            var label = labelValueAttribute.Label;
            var shortLabel = labelValueAttribute.ShortLabel;
            switch (TimeIdentifierFormat)
            {
                case TimeIdentifierFormatOption.Default:
                    return label;
                case TimeIdentifierFormatOption.None:
                    return Empty;
                case TimeIdentifierFormatOption.Short:
                    return shortLabel;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private enum YearFormatOption
        {
            Default,
            AcademicOrFiscal
        }

        private enum TimeIdentifierFormatOption
        {
            Default,
            None,
            Short
        }
    }
}