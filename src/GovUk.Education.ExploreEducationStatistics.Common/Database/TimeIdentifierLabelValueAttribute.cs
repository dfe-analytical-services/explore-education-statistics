using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TimeIdentifierLabelValueAttribute : EnumLabelValueAttribute
    {
        public TimePeriodLabelFormat Format { get; }
        public string ShortLabel { get; }

        public TimeIdentifierLabelValueAttribute(string label,
            string value,
            TimePeriodLabelFormat format = TimePeriodLabelFormat.Default,
            string shortLabel = null) : base(label, value)
        {
            Format = format;
            ShortLabel = shortLabel;
        }
    }
}