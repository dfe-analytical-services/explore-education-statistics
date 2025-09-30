using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public record TimePeriodRange
{
    public required TimePeriodRangeBound Start { get; set; }

    public required TimePeriodRangeBound End { get; set; }
}

public record TimePeriodRangeBound
{
    public required TimeIdentifier Code { get; set; }

    public required string Period { get; set; }

    public static TimePeriodRangeBound Create(TimePeriodMeta meta)
    {
        return new TimePeriodRangeBound { Code = meta.Code, Period = meta.Period };
    }
}
