using System.Text;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;

public record ReleaseSearchableDocumentDto
{
    public required Guid ReleaseId { get; init; }
    public required string ReleaseSlug { get; init; }
    public required Guid ReleaseVersionId { get; init; }
    public required Guid PublicationId { get; init; }
    public required string PublicationSlug { get; init; }
    public required string Summary { get; init; }
    public required string PublicationTitle { get; init; }
    public required DateTimeOffset Published { get; init; }
    public required Guid ThemeId { get; init; }
    public required string ThemeTitle { get; init; }
    public required string Type { get; init; }
    public required int TypeBoost { get; init; }
    public required string HtmlContent { get; init; }

    public static ReleaseSearchableDocumentDto FromModel(
        Publication publication,
        ReleaseCacheViewModel release
    ) =>
        new()
        {
            ReleaseId = release.ReleaseId,
            ReleaseSlug = release.Slug,
            ReleaseVersionId = release.Id,
            PublicationId = publication.Id,
            PublicationSlug = publication.Slug,
            Summary = publication.Summary,
            PublicationTitle = publication.Title,
            Published =
                release.Published
                ?? throw new ArgumentException("Release must have a published date"),
            ThemeId = publication.Theme.Id,
            ThemeTitle = publication.Theme.Title,
            Type = release.Type.ToString(),
            TypeBoost = release.Type.ToSearchDocumentTypeBoost(),
            HtmlContent = RenderSearchableHtmlContent(publication, release),
        };

    private static string RenderSearchableHtmlContent(
        Publication publication,
        ReleaseCacheViewModel release
    )
    {
        var sb = new StringBuilder();

        // HTML Content
        HtmlHeader(sb, publication.Title);

        // H1: Publication Title
        H1(sb, publication.Title);

        // H2: Release Title
        H2(sb, release.Title);

        // H3: "Summary"
        AddH3Section(sb, "Summary", release.SummarySection.Content);

        // H3: "Headlines"
        AddH3Section(sb, "Headlines", release.HeadlinesSection.Content);

        // Add content blocks
        foreach (var contentSection in release.Content)
        {
            AddH3Section(sb, contentSection.Heading, contentSection.Content);
        }

        HtmlFooter(sb);
        return sb.ToString().UseUnixNewLine(); // Ensure consistency regardless of the runtime platform
    }

    private static void AddH3Section(
        StringBuilder sb,
        string sectionTitle,
        List<IContentBlockViewModel> contentSection
    )
    {
        var htmlBlocks = ExtractHtmlBlocks(contentSection);
        if (string.IsNullOrEmpty(htmlBlocks))
        {
            return;
        }

        H3(sb, sectionTitle);
        sb.AppendLine(htmlBlocks);
    }

    private static void HtmlHeader(StringBuilder sb, string title) =>
        sb.AppendLine(
            $"""
            <html>
                <head>
                    <title>{title}</title>
                </head>
                <body>
            """
        );

    private static void H1(StringBuilder sb, string text) => sb.AppendLine($"<h1>{text}</h1>");

    private static void H2(StringBuilder sb, string text) => sb.AppendLine($"<h2>{text}</h2>");

    private static void H3(StringBuilder sb, string text) => sb.AppendLine($"<h3>{text}</h3>");

    private static void HtmlFooter(StringBuilder sb) =>
        sb.AppendLine(
            """
                </body>
            </html>
            """
        );

    private static string ExtractHtmlBlocks(List<IContentBlockViewModel> content) =>
        string.Join(
            Environment.NewLine,
            content.OfType<HtmlBlockViewModel>().OrderBy(c => c.Order).Select(c => c.Body)
        );
}
