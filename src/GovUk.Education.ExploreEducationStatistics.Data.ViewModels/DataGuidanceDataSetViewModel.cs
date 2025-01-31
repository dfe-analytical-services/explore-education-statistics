using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

public record DataGuidanceDataSetViewModel
{
    public Guid FileId { get; init; }

    public string Filename { get; init; } = string.Empty;

    public int Order { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Content { get; init; } = string.Empty;

    public TimePeriodLabels TimePeriods { get; init; } = new();

    public List<string> GeographicLevels { get; init; } = new();

    public List<LabelValue> Variables { get; init; } = new();

    public List<FootnoteViewModel> Footnotes { get; init; } = new();
}
