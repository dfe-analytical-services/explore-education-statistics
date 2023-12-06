namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class IndicatorChangeState
{
    public required string Id { get; set; }

    public required string Label { get; set; }

    // TODO: Change to Unit type
    public string Unit { get; set; } = string.Empty;

    public byte? DecimalPlaces { get; set; } = null;
}
