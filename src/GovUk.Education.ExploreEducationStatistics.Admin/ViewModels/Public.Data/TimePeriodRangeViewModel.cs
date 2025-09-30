#nullable enable
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record TimePeriodRangeViewModel
{
    public required string Start { get; set; }

    public required string End { get; set; }

    public static TimePeriodRangeViewModel Create(TimePeriodRange timePeriodRange)
    {
        return new TimePeriodRangeViewModel
        {
            Start = TimePeriodFormatter.FormatLabel(
                timePeriodRange.Start.Period,
                timePeriodRange.Start.Code
            ),
            End = TimePeriodFormatter.FormatLabel(
                timePeriodRange.End.Period,
                timePeriodRange.End.Code
            ),
        };
    }
}
