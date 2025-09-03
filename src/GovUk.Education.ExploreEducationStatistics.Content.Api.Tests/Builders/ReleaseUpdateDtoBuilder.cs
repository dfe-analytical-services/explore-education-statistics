using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;

public class ReleaseUpdateDtoBuilder
{
    private string? _summary;
    private DateTime? _date;

    public ReleaseUpdateDto Build() => new()
    {
        Summary = _summary ?? "Summary",
        Date = _date ?? new DateTime(2025, 09, 01)
    };

    public ReleaseUpdateDtoBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }

    public ReleaseUpdateDtoBuilder WithDate(DateTime date)
    {
        _date = date;
        return this;
    }
}
