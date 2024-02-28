using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class TimePeriodRange
{
    public required TimePeriodRangeBound Start { get; set; }

    public required TimePeriodRangeBound End { get; set; }
}

public class TimePeriodRangeBound
{
    public required TimeIdentifier Code { get; set; }

    public required int Year { get; set; }

    public static TimePeriodRangeBound Create(TimePeriodMeta meta)
    {
        return new TimePeriodRangeBound
        {
            Code = meta.Code,
            Year = meta.Year
        };
    }
}
