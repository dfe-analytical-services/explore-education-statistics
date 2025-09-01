using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;

public class PublicationThemeDtoBuilder
{
    private Guid? _id;
    private string? _summary;
    private string? _title;

    public PublicationThemeDto Build() => new()
    {
        Id = _id ?? Guid.NewGuid(),
        Summary = _summary ?? "Summary",
        Title = _title ?? "Title"
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
