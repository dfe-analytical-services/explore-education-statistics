using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record MethodologyVersionViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required DateTime? Published { get; init; }

    public required List<ContentSectionViewModel> Content { get; init; }

    public required List<ContentSectionViewModel> Annexes { get; init; }

    public required List<MethodologyNoteViewModel> Notes { get; init; }

    public required List<PublicationSummaryViewModel> Publications { get; init; }

    public static MethodologyVersionViewModel FromModel(
        MethodologyVersion methodologyVersion,
        List<PublicationSummaryViewModel> publications
    ) =>
        new()
        {
            Id = methodologyVersion.Id,
            Title = methodologyVersion.Title,
            Slug = methodologyVersion.Slug,
            Published = methodologyVersion.Published,
            Content = methodologyVersion
                .MethodologyContent.Content.OrderBy(contentSection => contentSection.Order)
                .Select(ContentSectionViewModel.FromModel)
                .ToList(),
            Annexes = methodologyVersion
                .MethodologyContent.Annexes.OrderBy(annexSection => annexSection.Order)
                .Select(ContentSectionViewModel.FromModel)
                .ToList(),
            Notes = methodologyVersion
                .Notes.OrderByDescending(note => note.DisplayDate)
                .Select(MethodologyNoteViewModel.FromModel)
                .ToList(),
            Publications = publications,
        };
}
