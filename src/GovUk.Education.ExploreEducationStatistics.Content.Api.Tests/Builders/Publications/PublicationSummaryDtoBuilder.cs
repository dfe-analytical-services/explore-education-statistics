using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;

public class PublicationSummaryDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private DateTimeOffset _published = DateTimeOffset.UtcNow;
    private string _slug = "Slug";
    private string _summary = "Summary";
    private string _title = "Title";

    public PublicationSummaryDto Build() =>
        new()
        {
            Id = _id,
            Published = _published,
            Slug = _slug,
            Summary = _summary,
            Title = _title,
        };

    public PublicationSummaryDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PublicationSummaryDtoBuilder WithPublished(DateTimeOffset published)
    {
        _published = published;
        return this;
    }

    public PublicationSummaryDtoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public PublicationSummaryDtoBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }

    public PublicationSummaryDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}
