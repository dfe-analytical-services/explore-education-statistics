#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class Indicator
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty; // displayed label
    public string Name { get; set; } = string.Empty; // csv column name
    public IndicatorUnit Unit { get; set; } = IndicatorUnit.None;
    public IndicatorGroup IndicatorGroup { get; set; } = null!;
    public Guid IndicatorGroupId { get; set; }
    public List<IndicatorFootnote> Footnotes { get; set; } = new();
    public int? DecimalPlaces { get; set; }
}
