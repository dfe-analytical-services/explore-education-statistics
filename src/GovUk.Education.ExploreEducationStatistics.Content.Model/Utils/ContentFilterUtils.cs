#nullable enable
using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Utils;

public static partial class ContentFilterUtils
{
    /// <summary>
    /// Markers are created by CKEditor and have characteristic start and end elements:
    /// comment, commentplaceholder and resolvedcomment.
    /// </summary>
    private const string CommentsFilterPattern =
        @"<\s*comment-(start|end)[^>]*(>[^>]*<\/\s*comment-(start|end)[^>]*>|\/>)|<\s*commentplaceholder-(start|end)[^>]*(>[^>]*<\/\s*commentplaceholder-(start|end)[^>]*>|\/>)|<\s*resolvedcomment-(start|end)[^>]*(>[^>]*<\/\s*resolvedcomment-(start|end)[^>]*>|\/>)";

    /// <summary>
    /// Source-generated regular expression for matching comments in content.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(CommentsFilterPattern)]
    public static partial Regex CommentsRegex();
}
