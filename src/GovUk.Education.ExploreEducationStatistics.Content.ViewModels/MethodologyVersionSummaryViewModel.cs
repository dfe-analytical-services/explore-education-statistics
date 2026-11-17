using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record MethodologyVersionSummaryViewModel
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public static MethodologyVersionSummaryViewModel FromModel(MethodologyVersion methodologyVersion) =>
        new()
        {
            Id = methodologyVersion.Id,
            Slug = methodologyVersion.Slug,
            Title = methodologyVersion.Title,
        };
}
