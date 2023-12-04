namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class IndicatorMeta
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public byte DecimalPlaces { get; set; }
}
