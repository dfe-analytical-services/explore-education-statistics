using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;

public static class TimePeriodRangeBoundMetaExtensions
{
    public static string ToLabel(this TimePeriodRangeBoundMeta meta) =>
        TimePeriodLabelFormatter.Format(meta.Period, meta.TimeIdentifier);
}
