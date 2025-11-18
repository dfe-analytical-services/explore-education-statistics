#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ReleaseNoteViewModel
{
    public required Guid Id { get; init; }

    public required string Reason { get; init; }

    public required DateTime On { get; init; }

    public static ReleaseNoteViewModel FromModel(Update update) =>
        new()
        {
            Id = update.Id,
            Reason = update.Reason,
            On = update.On,
        };
}
