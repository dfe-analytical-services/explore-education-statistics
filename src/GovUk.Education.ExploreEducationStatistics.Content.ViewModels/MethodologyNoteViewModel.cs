using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record MethodologyNoteViewModel
{
    public required Guid Id { get; init; }

    public required string Content { get; init; }

    public required DateTime DisplayDate { get; init; }

    public static MethodologyNoteViewModel FromModel(MethodologyNote methodologyNote) =>
        new()
        {
            Id = methodologyNote.Id,
            Content = methodologyNote.Content,
            DisplayDate = methodologyNote.DisplayDate,
        };
}
