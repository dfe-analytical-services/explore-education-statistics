using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class ReleaseDocumentBuilder
{
    private string? _summary;
    private string? _title;
    private Guid? _releaseVersionId;
    private Guid? _releaseId;
    private Guid? _publicationId;
    private Guid? _themeId;
    private string? _themeTitle;
    private PublishingOrganisation[]? _publishingOrganisations;

    public ReleaseDocument Build() =>
        new()
        {
            ReleaseId = _releaseId ?? new Guid("08228000-71de-4a1a-9adf-35f3d2bd061a"),
            ReleaseVersionId = _releaseVersionId ?? new Guid("a20f1068-4298-495d-8d80-c662ca72731d"),
            PublicationId = _publicationId ?? new Guid("8db69a42-ef47-441b-bed5-5e91d012194f"),
            ThemeId = _themeId ?? new Guid("498e449b-d093-43dc-959c-bc3a2c23fdfb"),
            ThemeTitle = _themeTitle ?? "Theme Title",
            Published = new DateTimeOffset(2025, 02, 21, 09, 24, 01, TimeSpan.Zero),
            PublicationTitle = _title ?? "Publication Title",
            Summary = _summary ?? "This is a summary.",
            ReleaseType = "Official Statistics",
            TypeBoost = 10,
            PublicationSlug = "publication-slug",
            ReleaseSlug = "release-slug",
            PublishingOrganisations =
                _publishingOrganisations
                ??
                [
                    new PublishingOrganisation
                    {
                        Id = new Guid("7cbcfe03-9f7e-478a-8512-a1a5e0ca793b"),
                        Title = "Department for Education",
                    },
                    new PublishingOrganisation
                    {
                        Id = new Guid("7c3252c6-6e94-4a34-8762-ff52aae5c0c0"),
                        Title = "Ofsted",
                    },
                ],
            HtmlContent = "<p>This is some Html Content</p>",
        };

    public ReleaseDocumentBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }

    public ReleaseDocumentBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ReleaseDocumentBuilder WithReleaseVersionId(Guid releaseVersionId)
    {
        _releaseVersionId = releaseVersionId;
        return this;
    }

    public ReleaseDocumentBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }

    public ReleaseDocumentBuilder WithPublicationId(Guid publicationId)
    {
        _publicationId = publicationId;
        return this;
    }

    public ReleaseDocumentBuilder WithThemeId(Guid themeId)
    {
        _themeId = themeId;
        return this;
    }

    public ReleaseDocumentBuilder WithThemeTitle(string themeTitle)
    {
        _themeTitle = themeTitle;
        return this;
    }

    public ReleaseDocumentBuilder WithPublishingOrganisations(params PublishingOrganisation[] publishingOrganisations)
    {
        _publishingOrganisations = publishingOrganisations;
        return this;
    }
}
