using System.Text;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseSearchViewModel
{
    public Guid ReleaseId { get; init; }
    public Guid ReleaseVersionId { get; init; }
    public DateTimeOffset Published { get; init; } = DateTimeOffset.MinValue;
    public string PublicationTitle { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string Theme { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;

    public int TypeBoost { get; init; }

    public string PublicationSlug { get; init; } = string.Empty;
    public string ReleaseSlug { get; init; } = string.Empty;

    public string HtmlContent { get; init; } = string.Empty;

    public ReleaseSearchViewModel(
        ReleaseCacheViewModel releaseVersion,
        PublicationCacheViewModel publication)
    {
        ReleaseId = releaseVersion.ReleaseId;
        ReleaseVersionId = releaseVersion.Id;
        Published = releaseVersion.Published ?? throw new ArgumentException("Release must have a published date");
        PublicationTitle = publication.Title;
        Summary = publication.Summary;
        Theme = publication.Theme.Title;
        Type = releaseVersion.Type.ToString();
        TypeBoost = releaseVersion.Type.ToSearchDocumentTypeBoost();
        PublicationSlug = publication.Slug;
        ReleaseSlug = releaseVersion.Slug;
        HtmlContent = RenderSearchableHtmlContent(publication, releaseVersion);
    }

    private static string RenderSearchableHtmlContent(
        PublicationCacheViewModel publication,
        ReleaseCacheViewModel releaseContent)
    {
        var sb = new StringBuilder();

        // HTML Content
        HtmlHeader(sb, publication.Title);
        
        // H1: Publication Title
        H1(sb, publication.Title);
        
        // H2: Release Title
        H2(sb, releaseContent.Title);
        
        // H3: "Summary"
        AddH3Section(sb, "Summary", releaseContent.SummarySection.Content);
        
        // H3: "Headlines"
        AddH3Section(sb, "Headlines", releaseContent.HeadlinesSection.Content);
        
        // Add content blocks
        foreach (var contentSection in releaseContent.Content)
        {
            AddH3Section(sb, contentSection.Heading, contentSection.Content);
        }

        HtmlFooter(sb);
        return sb.ToString().UseUnixNewLine(); // Ensure consistency regardless of the runtime platform
    }

    private static void AddH3Section(StringBuilder sb, string sectionTitle, List<IContentBlockViewModel> contentSection)
    {
        var htmlBlocks = ExtractHtmlBlocks(contentSection);
        if (string.IsNullOrEmpty(htmlBlocks))
        {
            return;
        }
            
        H3(sb, sectionTitle);
        sb.AppendLine(htmlBlocks);
    }
    
    private static void HtmlHeader(StringBuilder sb, string title) => sb.AppendLine(
        $"""
         <html>
             <head>
                 <title>{title}</title>
             </head>
             <body>
         """);
    private static void H1(StringBuilder sb, string text) => sb.AppendLine($"<h1>{text}</h1>");
    private static void H2(StringBuilder sb, string text) => sb.AppendLine($"<h2>{text}</h2>");
    private static void H3(StringBuilder sb, string text) => sb.AppendLine($"<h3>{text}</h3>");
    
    private static void HtmlFooter(StringBuilder sb) => sb.AppendLine(  
        """
            </body>
        </html>
        """);
    
    private static string ExtractHtmlBlocks(List<IContentBlockViewModel> content) =>
        string.Join(Environment.NewLine,
            content
                .OfType<HtmlBlockViewModel>()
                .OrderBy(c => c.Order)
                .Select(c => c.Body));
}
