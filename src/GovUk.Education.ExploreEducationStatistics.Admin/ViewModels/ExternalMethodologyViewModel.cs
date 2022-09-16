#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record ExternalMethodologyViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public ExternalMethodologyViewModel()
    {
    }

    public ExternalMethodologyViewModel(ExternalMethodology externalMethodology)
    {
        Title = externalMethodology.Title;
        Url = externalMethodology.Url;
    }
}
