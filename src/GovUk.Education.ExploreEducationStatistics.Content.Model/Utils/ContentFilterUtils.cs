#nullable enable
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Utils
{
    public static class ContentFilterUtils
    {
        /// <summary>
        /// Strips out anything that looks like a comment.
        /// </summary>
        public static readonly string CommentsFilterPattern = MarkerElementPattern(
            "comment",
            "commentplaceholder",
            "resolvedcomment"
        );

        private static string MarkerElementPattern(params string[] elements)
        {
            // Markers are created by CKEditor and have characteristic start and end elements
            return elements
                .Select(el =>  $@"<\s*{el}-(start|end)[^>]*(>[^>]*<\/\s*{el}-(start|end)[^>]*>|\/>)")
                .JoinToString('|');
        }
    }
}