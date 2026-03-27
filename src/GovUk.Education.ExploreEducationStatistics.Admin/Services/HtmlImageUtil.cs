#nullable enable
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public static partial class HtmlImageUtil
{
    [GeneratedRegex("^/api/methodologies/{methodologyId}/images/(.+)$")]
    private static partial Regex MethodologyImagesPattern();

    [GeneratedRegex("^/api/releases/{(releaseId|releaseVersionId)}/images/(.+)$")]
    private static partial Regex ReleaseImagesPattern();

    public static List<Guid> GetMethodologyImages(string? htmlContent) =>
        string.IsNullOrEmpty(htmlContent) ? [] : GetImages(htmlContent, MethodologyImagesPattern());

    // TODO EES-5901 - migrate all content placeholders to be "releaseVersionId" and then remove the legacy
    // "releaseId" checks below.
    public static List<Guid> GetReleaseImages(string? htmlContent) =>
        string.IsNullOrEmpty(htmlContent) ? [] : GetImages(htmlContent, ReleaseImagesPattern());

    private static List<Guid> GetImages(string htmlContent, Regex idCapturingRegex)
    {
        var htmlDocument = ParseContentAsHtml(htmlContent);
        var imageIds = htmlDocument
            .DocumentNode.SelectNodes("//img")
            .Select(node => node.Attributes["src"].Value)
            .Select(srcVal => idCapturingRegex.Match(srcVal))
            .Where(match => match.Success)
            .Select(match => match.Groups[^1].Value)
            .ToList();

        // Convert to Guids, removing any that are malformed
        return ParseGuids(imageIds);
    }

    private static HtmlDocument ParseContentAsHtml(string htmlContent)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);
        htmlDoc.OptionEmptyCollection = true;
        return htmlDoc;
    }

    private static List<Guid> ParseGuids(IEnumerable<string> input) =>
        input
            .Select(s => Guid.TryParse(s, out var g) ? g : (Guid?)null)
            .Where(g => g.HasValue)
            .Select(g => g!.Value)
            .ToList();
}
