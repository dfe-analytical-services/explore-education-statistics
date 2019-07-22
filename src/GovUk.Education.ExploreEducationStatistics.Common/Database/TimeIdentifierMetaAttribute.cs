using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Common.Database
{
    [AttributeUsage(AttributeTargets.Field)]
    public class TimeIdentifierMetaAttribute : EnumLabelValueAttribute
    {
        public TimePeriodLabelFormat Format { get; }
        public string ShortLabel { get; }

        public TimeIdentifierCategory Category { get; }

        public TimeIdentifierMetaAttribute(string label,
            string value,
            TimeIdentifierCategory category,
            TimePeriodLabelFormat format = TimePeriodLabelFormat.Default,
            string shortLabel = null) : base(label, value)
        {
            Format = format;
            ShortLabel = shortLabel;
            Category = category;
        }
    }
}