using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;

public class PublicationReleaseEntryDtoBuilder
{
    private Guid _releaseId = Guid.NewGuid();
    private bool _isLatestRelease;
    private string? _label = "Label";
    private DateTime _lastUpdated = new(2025, 9, 1, 8, 30, 0, DateTimeKind.Utc);
    private DateTime _published = new(2025, 8, 1, 8, 30, 0, DateTimeKind.Utc);
    private string _slug = "Slug";
    private string _title = "Title";
    private string _coverageTitle = "Calendar year";
    private string _yearTitle = "2024";

    public PublicationReleaseEntryDto Build() => new()
    {
        ReleaseId = _releaseId,
        IsLatestRelease = _isLatestRelease,
        Label = _label,
        LastUpdated = _lastUpdated,
        Published = _published,
        Slug = _slug,
        Title = _title,
        CoverageTitle = _coverageTitle,
        YearTitle = _yearTitle
    };

    public PublicationReleaseEntryDtoBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }

    public PublicationReleaseEntryDtoBuilder WithIsLatestRelease(bool isLatestRelease)
    {
        _isLatestRelease = isLatestRelease;
        return this;
    }

    public PublicationReleaseEntryDtoBuilder WithLabel(string? label)
    {
        _label = label;
        return this;
    }

    public PublicationReleaseEntryDtoBuilder WithLastUpdated(DateTime lastUpdated)
    {
        _lastUpdated = lastUpdated;
        return this;
    }

    public PublicationReleaseEntryDtoBuilder WithPublished(DateTime published)
    {
        _published = published;
        return this;
    }

    public PublicationReleaseEntryDtoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public PublicationReleaseEntryDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public PublicationReleaseEntryDtoBuilder WithCoverageTitle(string coverageTitle)
    {
        _coverageTitle = coverageTitle;
        return this;
    }

    public PublicationReleaseEntryDtoBuilder WithYearTitle(string yearTitle)
    {
        _yearTitle = yearTitle;
        return this;
    }
}
