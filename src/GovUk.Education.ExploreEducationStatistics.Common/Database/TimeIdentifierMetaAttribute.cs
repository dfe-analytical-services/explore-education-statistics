using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TimeIdentifierMetaAttribute : EnumLabelValueAttribute
    {
        public TimePeriodYearFormat YearFormat { get; }
        public TimePeriodLabelFormat LabelFormat { get; }
        public string ShortLabel { get; }

        public TimeIdentifierCategory Category { get; }

        public TimeIdentifierMetaAttribute(string label,
            string value,
            TimeIdentifierCategory category,
            TimePeriodYearFormat yearFormat = TimePeriodYearFormat.Default,
            TimePeriodLabelFormat labelFormat = TimePeriodLabelFormat.FullLabel,
            string shortLabel = null) : base(label, value)
        {
            YearFormat = yearFormat;
            LabelFormat = labelFormat;
            ShortLabel = shortLabel;
            Category = category;
        }
    }
}