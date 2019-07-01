using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class TimePeriodLabelFormatter
    {
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
            var labelValueAttribute = timeIdentifier.GetEnumAttribute<TimeIdentifierLabelValueAttribute>();
            var formatter = Formatters[labelValueAttribute.Format];
            return formatter.Format(year, labelValueAttribute.Label, labelValueAttribute.ShortLabel);
        }

        private string Format(int year, string label, string shortLabel)
        {
            var formattedYear = FormatYear(year);
            var formattedLabel = FormatLabel(label, shortLabel);
            return string.IsNullOrEmpty(formattedLabel) ? formattedYear : $"{formattedYear} {formattedLabel}";
        }

        private string FormatLabel(string label, string shortLabel)
        {
            switch (TimeIdentifierFormat)
            {
                case TimeIdentifierFormatOption.Default:
                    return label;
                case TimeIdentifierFormatOption.None:
                    return string.Empty;
                case TimeIdentifierFormatOption.Short:
                    return shortLabel;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string FormatYear(int year)
        {
            switch (YearFormat)
            {
                case YearFormatOption.Default:
                    return year.ToString();
                case YearFormatOption.AcademicOrFiscal:
                    return $"{year}/{(year + 1).ToString().Substring(2)}";
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