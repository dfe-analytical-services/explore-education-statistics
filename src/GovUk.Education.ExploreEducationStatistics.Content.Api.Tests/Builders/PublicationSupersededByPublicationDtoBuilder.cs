using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;

public class PublicationSupersededByPublicationDtoBuilder
{
    private Guid? _id;
    private string? _slug;
    private string? _title;

    public PublicationSupersededByPublicationDto Build() => new()
    {
        Id = _id ?? Guid.NewGuid(),
        Slug = _slug ?? "Slug",
        Title = _title ?? "Title"
    };

    public PublicationSupersededByPublicationDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PublicationSupersededByPublicationDtoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public PublicationSupersededByPublicationDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}
