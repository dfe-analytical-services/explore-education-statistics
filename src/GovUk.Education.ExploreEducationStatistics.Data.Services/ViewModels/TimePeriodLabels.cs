#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class TimePeriodLabels
    {
        public string From { get; } = string.Empty;
        public string To { get; } = string.Empty;

        public TimePeriodLabels()
        {
        }

        public TimePeriodLabels(string from, string to)
        {
            From = from;
            To = to;
        }

        public string ToLabel()
        {
            var hasFrom = !From.IsNullOrEmpty();
            var hasTo = !To.IsNullOrEmpty();

            if (hasFrom && hasTo)
            {
                return To == From ? To : $"{From} to {To}";
            }

            if (!hasFrom && !hasTo)
            {
                return string.Empty;
            }

            return hasTo ? To : From;
        }
    }
}
