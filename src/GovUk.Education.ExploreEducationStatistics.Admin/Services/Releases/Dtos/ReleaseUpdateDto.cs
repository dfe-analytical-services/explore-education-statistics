#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases.Dtos;

public record ReleaseUpdateDto
{
    public required Guid Id { get; init; }

    public required DateTime Date { get; init; }

    public required string Summary { get; init; }

    public static ReleaseUpdateDto FromModel(Update update) =>
        new()
        {
            Id = update.Id,
            Date = update.On,
            Summary = update.Reason,
        };
}
