#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public record IdTitleSlugViewModel : IdTitleViewModel
{
    public string? Slug { get; init; }
}
