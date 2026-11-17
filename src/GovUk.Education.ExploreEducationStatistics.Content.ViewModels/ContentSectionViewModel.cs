using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ContentSectionViewModel
{
    public required Guid Id { get; init; }

    public required int Order { get; init; }

    public required string Heading { get; init; }

    public required List<IContentBlockViewModel> Content { get; init; }

    public static ContentSectionViewModel FromModel(ContentSection contentSection) =>
        new()
        {
            Id = contentSection.Id,
            Order = contentSection.Order,
            Heading = contentSection.Heading ?? string.Empty,
            Content = contentSection
                .Content.OrderBy(contentBlock => contentBlock.Order)
                .Select(IContentBlockViewModel.FromModel)
                .ToList(),
        };
}
