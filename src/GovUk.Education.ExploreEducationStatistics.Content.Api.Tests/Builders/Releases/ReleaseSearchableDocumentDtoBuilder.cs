using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;

public class ReleaseSearchableDocumentDtoBuilder
{
    private Guid _releaseId = Guid.NewGuid();
    private string _releaseSlug = "Release slug";
    private Guid _releaseVersionId = Guid.NewGuid();
    private Guid _publicationId = Guid.NewGuid();
    private string _publicationSlug = "Publication slug";
    private string _publicationSummary = "Publication summary";
    private string _publicationTitle = "Publication title";
    private DateTimeOffset _published = new DateTime(2025, 09, 01, 08, 30, 00, DateTimeKind.Utc);
    private Guid _themeId = Guid.NewGuid();
    private string _themeTitle = "Theme title";
    private ReleaseType _type = ReleaseType.OfficialStatistics;
    private string _htmlContent = "HTML content";

    public ReleaseSearchableDocumentDto Build() => new()
    {
        ReleaseId = _releaseId,
        ReleaseSlug = _releaseSlug,
        ReleaseVersionId = _releaseVersionId,
        PublicationId = _publicationId,
        PublicationSlug = _publicationSlug,
        Summary = _publicationSummary,
        PublicationTitle = _publicationTitle,
        Published = _published,
        ThemeId = _themeId,
        ThemeTitle = _themeTitle,
        Type = _type.ToString(),
        TypeBoost = _type.ToSearchDocumentTypeBoost(),
        HtmlContent = _htmlContent
    };

    public ReleaseSearchableDocumentDtoBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithReleaseSlug(string releaseSlug)
    {
        _releaseSlug = releaseSlug;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithReleaseVersionId(Guid releaseVersionId)
    {
        _releaseVersionId = releaseVersionId;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithPublicationId(Guid publicationId)
    {
        _publicationId = publicationId;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithPublicationSlug(string publicationSlug)
    {
        _publicationSlug = publicationSlug;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithPublicationSummary(string publicationSummary)
    {
        _publicationSummary = publicationSummary;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithPublicationTitle(string publicationTitle)
    {
        _publicationTitle = publicationTitle;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithPublished(DateTimeOffset published)
    {
        _published = published;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithThemeId(Guid themeId)
    {
        _themeId = themeId;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithThemeTitle(string themeTitle)
    {
        _themeTitle = themeTitle;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithType(ReleaseType type)
    {
        _type = type;
        return this;
    }

    public ReleaseSearchableDocumentDtoBuilder WithHtmlContent(string htmlContent)
    {
        _htmlContent = htmlContent;
        return this;
    }
}
