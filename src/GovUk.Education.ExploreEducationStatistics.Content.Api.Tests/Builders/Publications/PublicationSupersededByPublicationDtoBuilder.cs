using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;

public class PublicationSupersededByPublicationDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _slug = "Slug";
    private string _title = "Title";

    public PublicationSupersededByPublicationDto Build() => new()
    {
        Id = _id,
        Slug = _slug,
        Title = _title
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
