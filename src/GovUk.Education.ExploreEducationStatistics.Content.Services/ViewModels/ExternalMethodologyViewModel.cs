#nullable enable

using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record ExternalMethodologyViewModel(
    string Title,
    string Url)
{
    public ExternalMethodologyViewModel(ExternalMethodology externalMethodology) : this(
        Title: externalMethodology.Title,
        Url: externalMethodology.Url
    )
    {
    }
}
