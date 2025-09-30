using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;

public class PublicationThemeDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _summary = "Summary";
    private string _title = "Title";

    public PublicationThemeDto Build() => new()
    {
        Id = _id,
        Summary = _summary,
        Title = _title
    };

    public PublicationThemeDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PublicationThemeDtoBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }

    public PublicationThemeDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}
