using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;

public class ReleaseVersionSummaryDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _releaseId = Guid.NewGuid();
    private bool _isLatestRelease = true;
    private string? _label = "Label";
    private DateTime _lastUpdated = new(2025, 9, 1, 8, 30, 0, DateTimeKind.Utc);
    private DateTime _published = new(2025, 8, 1, 8, 30, 0, DateTimeKind.Utc);
    private PublishingOrganisationDto[] _publishingOrganisations = [new PublishingOrganisationDtoBuilder().Build()];
    private string _slug = "Slug";
    private string _title = "Title";
    private string _coverageTitle = "Calendar year";
    private string _yearTitle = "2024";
    private ReleaseType _type = ReleaseType.OfficialStatistics;
    private string? _preReleaseAccessList = "Pre-release access list";
    private int _updateCount = 1;

    public ReleaseVersionSummaryDto Build() =>
        new()
        {
            Id = _id,
            ReleaseId = _releaseId,
            IsLatestRelease = _isLatestRelease,
            Label = _label,
            LastUpdated = _lastUpdated,
            Published = _published,
            PublishingOrganisations = _publishingOrganisations,
            Slug = _slug,
            Title = _title,
            CoverageTitle = _coverageTitle,
            YearTitle = _yearTitle,
            Type = _type,
            PreReleaseAccessList = _preReleaseAccessList,
            UpdateCount = _updateCount,
        };

    public ReleaseVersionSummaryDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithIsLatestRelease(bool isLatestRelease)
    {
        _isLatestRelease = isLatestRelease;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithLabel(string? label)
    {
        _label = label;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithLastUpdated(DateTime lastUpdated)
    {
        _lastUpdated = lastUpdated;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithPublished(DateTime published)
    {
        _published = published;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithPublishingOrganisations(
        PublishingOrganisationDto[] publishingOrganisations
    )
    {
        _publishingOrganisations = publishingOrganisations;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithCoverageTitle(string coverageTitle)
    {
        _coverageTitle = coverageTitle;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithYearTitle(string yearTitle)
    {
        _yearTitle = yearTitle;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithType(ReleaseType type)
    {
        _type = type;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithPreReleaseAccessList(string? preReleaseAccessList)
    {
        _preReleaseAccessList = preReleaseAccessList;
        return this;
    }

    public ReleaseVersionSummaryDtoBuilder WithUpdateCount(int updateCount)
    {
        _updateCount = updateCount;
        return this;
    }
}

public class PublishingOrganisationDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _title = "Title";
    private string _url = "Url";

    public PublishingOrganisationDto Build() =>
        new()
        {
            Id = _id,
            Title = _title,
            Url = _url,
        };

    public PublishingOrganisationDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PublishingOrganisationDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public PublishingOrganisationDtoBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }
}
