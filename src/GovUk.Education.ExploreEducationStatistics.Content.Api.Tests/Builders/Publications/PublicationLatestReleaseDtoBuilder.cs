using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;

public class PublicationLatestReleaseDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _slug = "Slug";
    private string _title = "Title";

    public PublicationLatestReleaseDto Build() => new()
    {
        Id = _id,
        Slug = _slug,
        Title = _title
    };

    public PublicationLatestReleaseDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PublicationLatestReleaseDtoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public PublicationLatestReleaseDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }
}
