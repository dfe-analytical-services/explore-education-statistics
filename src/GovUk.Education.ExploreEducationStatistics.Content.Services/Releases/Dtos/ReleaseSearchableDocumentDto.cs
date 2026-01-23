using System.Text;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;

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

    public static ReleaseSearchableDocumentDto FromReleaseVersion(ReleaseVersion releaseVersion) =>
        new()
        {
            ReleaseId = releaseVersion.ReleaseId,
            ReleaseSlug = releaseVersion.Release.Slug,
            ReleaseVersionId = releaseVersion.Id,
            PublicationId = releaseVersion.Release.PublicationId,
            PublicationSlug = releaseVersion.Release.Publication.Slug,
            Summary = releaseVersion.Release.Publication.Summary,
            PublicationTitle = releaseVersion.Release.Publication.Title,
            Published =
                releaseVersion.PublishedDisplayDate
                ?? throw new ArgumentException("Release must have a published date"),
            ThemeId = releaseVersion.Release.Publication.ThemeId,
            ThemeTitle = releaseVersion.Release.Publication.Theme.Title,
            Type = releaseVersion.Type.ToString(),
            TypeBoost = releaseVersion.Type.ToSearchDocumentTypeBoost(),
            HtmlContent = RenderSearchableHtmlContent(releaseVersion),
        };

    private static string RenderSearchableHtmlContent(ReleaseVersion releaseVersion)
    {
        var sb = new StringBuilder();

        HtmlHeader(sb, releaseVersion.Release.Publication.Title);

        // H1: Publication Title
        H1(sb, releaseVersion.Release.Publication.Title);

        // H2: Release Title
        H2(sb, releaseVersion.Release.Title);

        // H3: Summary section
        if (releaseVersion.SummarySection != null)
        {
            AddH3Section(sb, "Summary", releaseVersion.SummarySection);
        }

        // H3: Headlines section
        if (releaseVersion.HeadlinesSection != null)
        {
            AddH3Section(sb, "Headlines", releaseVersion.HeadlinesSection);
        }

        // Add H3 generic content sections
        var contentSections = releaseVersion.GenericContent.OrderBy(section => section.Order).ToList();
        foreach (var contentSection in contentSections)
        {
            AddH3Section(sb, contentSection.Heading ?? "", contentSection);
        }

        HtmlFooter(sb);

        return sb.ToString().UseUnixNewLine(); // Ensure consistency regardless of the runtime platform
    }

    private static void AddH3Section(StringBuilder sb, string sectionTitle, ContentSection contentSection)
    {
        var htmlBlocks = contentSection.Content.OfType<HtmlBlock>().OrderBy(hb => hb.Order).ToList();

        if (htmlBlocks.Count == 0)
        {
            return;
        }

        var htmlBlockBodies = htmlBlocks.Select(hb => RemoveComments(hb.Body)).JoinToString(Environment.NewLine);

        H3(sb, sectionTitle);
        sb.AppendLine(htmlBlockBodies);
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

    private static string RemoveComments(string input) =>
        ContentFilterUtils.CommentsRegex().Replace(input, string.Empty);
}
