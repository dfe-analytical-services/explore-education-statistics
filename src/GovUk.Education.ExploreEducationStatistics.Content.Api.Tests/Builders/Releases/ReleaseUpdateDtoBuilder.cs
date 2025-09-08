using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;

public class ReleaseUpdateDtoBuilder
{
    private string _summary = "Summary";
    private DateTime _date = new(2025, 09, 01);

    public ReleaseUpdateDto Build() => new()
    {
        Summary = _summary,
        Date = _date
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
