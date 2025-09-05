using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders;

public class PublicationDtoBuilder
{
    private Guid? _id;
    private string? _slug;
    private string? _summary;
    private string? _title;
    private PublicationContactDto? _contact;
    private PublicationLatestReleaseDto? _latestRelease;
    private PublicationSupersededByPublicationDto? _supersededByPublication;
    private PublicationThemeDto? _theme;

    public PublicationDto Build() => new()
    {
        Id = _id ?? Guid.NewGuid(),
        Slug = _slug ?? "Slug",
        Summary = _summary ?? "Summary",
        Title = _title ?? "Title",
        Contact = _contact ?? new PublicationContactDtoBuilder().Build(),
        LatestRelease = _latestRelease ?? new PublicationLatestReleaseDtoBuilder().Build(),
        SupersededByPublication =
            _supersededByPublication ?? new PublicationSupersededByPublicationDtoBuilder().Build(),
        Theme = _theme ?? new PublicationThemeDtoBuilder().Build()
    };

    public PublicationDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PublicationDtoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public PublicationDtoBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }


    public PublicationDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public PublicationDtoBuilder WithContact(PublicationContactDto contact)
    {
        _contact = contact;
        return this;
    }

    public PublicationDtoBuilder WithLatestRelease(PublicationLatestReleaseDto latestRelease)
    {
        _latestRelease = latestRelease;
        return this;
    }

    public PublicationDtoBuilder WithSupersededByPublication(
        PublicationSupersededByPublicationDto? supersededByPublication)
    {
        _supersededByPublication = supersededByPublication;
        return this;
    }

    public PublicationDtoBuilder WithTheme(PublicationThemeDto theme)
    {
        _theme = theme;
        return this;
    }
}
