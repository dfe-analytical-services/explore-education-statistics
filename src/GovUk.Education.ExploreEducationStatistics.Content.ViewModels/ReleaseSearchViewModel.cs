using System.Text;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

public record ReleaseSearchViewModel
{
    public Guid ReleaseId { get; init; }
    public DateTimeOffset Published { get; init; }
    public string PublicationTitle { get; init; }
    public string Summary { get; init; }
    public string Theme { get; init; }
    public string Type { get; init; }

    public int TypeBoost { get; init; }

    public string PublicationSlug { get; init; }
    public string ReleaseSlug { get; init; }

    public string HtmlContent { get; init; }

    public ReleaseSearchViewModel(
        ReleaseCacheViewModel release,
        PublicationCacheViewModel publication)
    {
        ReleaseId = release.Id;
        Published = release.Published ?? throw new ArgumentException("Release must have a published date");
        PublicationTitle = publication.Title;
        Summary = ExtractHtmlBlocks(release.SummarySection.Content).StripHtml();
        Theme = publication.Theme.Title;
        Type = release.Type.ToDisplayString();
        TypeBoost = release.Type.ToTypeBoost();
        PublicationSlug = publication.Slug;
        ReleaseSlug = release.Slug;
        HtmlContent = RenderSearchableHtmlContent(publication, release);
    }

    private string RenderSearchableHtmlContent(
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

    private void AddH3Section(StringBuilder sb, string sectionTitle, List<IContentBlockViewModel> contentSection)
    {
        var htmlBlocks = ExtractHtmlBlocks(contentSection);
        if (string.IsNullOrEmpty(htmlBlocks)) return;
            
        H3(sb, sectionTitle);
        sb.AppendLine(htmlBlocks);
    }
    
    private void HtmlHeader(StringBuilder sb, string title) => sb.AppendLine(
        $"""
         <html>
             <head>
                 <title>{title}</title>
             </head>
             <body>
         """);
    private void H1(StringBuilder sb, string text) => sb.AppendLine($"<h1>{text}</h1>");
    private void H2(StringBuilder sb, string text) => sb.AppendLine($"<h2>{text}</h2>");
    private void H3(StringBuilder sb, string text) => sb.AppendLine($"<h3>{text}</h3>");
    
    private void HtmlFooter(StringBuilder sb) => sb.AppendLine(  
        """
            </body>
        </html>
        """);
    
    private string ExtractHtmlBlocks(List<IContentBlockViewModel> content) =>
        string.Join(Environment.NewLine,
            content
                .OfType<HtmlBlockViewModel>()
                .OrderBy(c => c.Order)
                .Select(c => c.Body));
}
