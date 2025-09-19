using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;

public class LegacyPublicationReleaseEntryDtoBuilder
{
    private string _title = "Title";
    private string _url = "Url";

    public LegacyPublicationReleaseEntryDto Build() => new()
    {
        Title = _title,
        Url = _url
    };

    public LegacyPublicationReleaseEntryDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public LegacyPublicationReleaseEntryDtoBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }
}
