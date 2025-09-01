using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;

public class PublicationLatestReleaseDtoBuilder
{
    private Guid? _id;
    private string? _slug;
    private string? _title;

    public PublicationLatestReleaseDto Build() => new()
    {
        Id = _id ?? Guid.NewGuid(),
        Slug = _slug ?? "Slug",
        Title = _title ?? "Title"
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
