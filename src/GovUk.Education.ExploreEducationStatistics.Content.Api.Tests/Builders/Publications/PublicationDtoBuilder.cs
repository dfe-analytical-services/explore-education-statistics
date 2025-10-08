using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Publications;

public class PublicationDtoBuilder
{
    private Guid _id = Guid.NewGuid();

    private string _slug = "Slug";

    private string _summary = "Summary";

    private string _title = "Title";

    private PublicationContactDto _contact = new PublicationContactDtoBuilder().Build();

    private PublicationLatestReleaseDto _latestRelease = new PublicationLatestReleaseDtoBuilder().Build();

    private PublicationNextReleaseDateDto? _nextReleaseDate = new PublicationNextReleaseDateDtoBuilder().Build();

    private PublicationSupersededByPublicationDto? _supersededByPublication =
        new PublicationSupersededByPublicationDtoBuilder().Build();

    private PublicationThemeDto _theme = new PublicationThemeDtoBuilder().Build();

    public PublicationDto Build() =>
        new()
        {
            Id = _id,
            Slug = _slug,
            Summary = _summary,
            Title = _title,
            Contact = _contact,
            LatestRelease = _latestRelease,
            NextReleaseDate = _nextReleaseDate,
            SupersededByPublication = _supersededByPublication,
            Theme = _theme,
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

    public PublicationDtoBuilder WithNextReleaseDate(PublicationNextReleaseDateDto? nextReleaseDate)
    {
        _nextReleaseDate = nextReleaseDate;
        return this;
    }

    public PublicationDtoBuilder WithSupersededByPublication(
        PublicationSupersededByPublicationDto? supersededByPublication
    )
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
