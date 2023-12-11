using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class IndicatorChangeState
{
    public required string Id { get; set; }

    public required string Label { get; set; }

    public IndicatorUnit? Unit { get; set; }

    public byte? DecimalPlaces { get; set; }
}
