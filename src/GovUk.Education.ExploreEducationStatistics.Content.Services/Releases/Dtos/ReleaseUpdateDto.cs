using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

public record ReleaseUpdateDto
{
    public required DateTime Date { get; init; }

    public required string Summary { get; init; }

    public static ReleaseUpdateDto FromUpdate(Update update) =>
        new()
        {
            Date = update.On,
            Summary = update.Reason
        };
}
