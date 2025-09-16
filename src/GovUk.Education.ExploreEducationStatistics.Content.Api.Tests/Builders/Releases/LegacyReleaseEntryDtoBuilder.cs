using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;

public class LegacyReleaseEntryDtoBuilder
{
    private string _title = "Title";
    private string _url = "Url";

    public LegacyReleaseEntryDto Build() => new()
    {
        Title = _title,
        Url = _url
    };

    public LegacyReleaseEntryDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public LegacyReleaseEntryDtoBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }
}
