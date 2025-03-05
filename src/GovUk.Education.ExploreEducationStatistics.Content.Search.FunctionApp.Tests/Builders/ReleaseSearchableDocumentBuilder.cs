using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class ReleaseSearchableDocumentBuilder
{
    private string? _summary;
    private string? _title;
    private Guid? _releaseVersionId;
    private Guid? _releaseId;

    public ReleaseSearchableDocument Build() => new()
    {
        ReleaseId = _releaseId ?? new Guid("11223344-5566-7788-9900-123456789abc"),
        ReleaseVersionId = _releaseVersionId ?? new Guid("12345678-1234-1234-1234-123456789abc"),
        Published = new DateTimeOffset(2025, 02, 21, 09, 24, 01, TimeSpan.Zero),
        PublicationTitle = _title ?? "Publication Title",
        Summary = _summary ?? "This is a summary.",
        Theme = "Theme",
        ReleaseType = "Official Statistics",
        TypeBoost = 10,
        PublicationSlug = "publication-slug",
        ReleaseSlug = "release-slug",
        HtmlContent = "<p>This is some Html Content</p>",
    };

    public ReleaseSearchableDocumentBuilder WithSummary(string summary)
    {
        _summary = summary;
        return this;
    }
    
    public ReleaseSearchableDocumentBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ReleaseSearchableDocumentBuilder WithReleaseVersionId(Guid releaseVersionId)
    {
        _releaseVersionId = releaseVersionId;
        return this;
    }

    public ReleaseSearchableDocumentBuilder WithReleaseId(Guid releaseId)
    {
        _releaseId = releaseId;
        return this;
    }
}
