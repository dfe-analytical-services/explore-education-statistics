#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

public record ReleaseNoteViewModel
{
    public required Guid Id { get; init; }

    public required DateOnly On { get; init; }

    public required string Reason { get; init; }

    public static ReleaseNoteViewModel FromUpdate(Update update) =>
        new()
        {
            Id = update.Id,
            On = new DateTimeOffset(update.On, TimeSpan.Zero).ToUkDateOnly(),
            Reason = update.Reason,
        };
}
