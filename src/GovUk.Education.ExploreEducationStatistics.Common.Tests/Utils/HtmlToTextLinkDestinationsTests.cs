#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public class HtmlToTextLinkDestinationsTests
{
    public class HtmlToTextTests
    {
        [Theory]
        [InlineData(
            "<p>Read <a href=\"https://example.com\">report</a>.</p>",
            "Read report (https://example.com).",
            "Read report."
        )]
        [InlineData(
            "<p><a href=\"http://example.com/a(b)\">first</a> &amp; <a href=\"https://example.com?x=1&amp;y=2\">second</a>.</p>",
            "first (http://example.com/a(b))& second (https://example.com?x=1&y=2).",
            "first& second."
        )]
        [InlineData(
            "<p>See <a href=\"/methodology\">methodology</a>, <a href=\"#notes\">notes</a> or <a href=\"mailto:statistics@example.com\">team</a>.</p>",
            "See methodology (/methodology), notes (#notes) or team (mailto:statistics@example.com).",
            "See methodology, notes or team."
        )]
        [InlineData(
            "<p>Keep (https://intentional.example) and <a href=\"https://example.com\"><strong>label (detail)</strong></a>.</p>",
            "Keep (https://intentional.example) andlabel (detail) (https://example.com).",
            "Keep (https://intentional.example) andlabel (detail)."
        )]
        [InlineData(
            "<p><a>Missing href</a> and <a href=\"\">empty href</a>.</p>",
            "Missing href and empty href.",
            "Missing href and empty href."
        )]
        [InlineData(
            "No link (https://intentional.example).",
            "No link (https://intentional.example).",
            "No link (https://intentional.example)."
        )]
        public void PreservesLabelsAndProseWhileOmittingOnlyGeneratedDestinations(
            string html,
            string withDestinations,
            string withoutDestinations
        )
        {
            Assert.Equal(withDestinations, HtmlToTextUtils.HtmlToText(html));
            Assert.Equal(withDestinations, HtmlToTextUtils.HtmlToText(html, includeLinkDestinations: true));
            Assert.Equal(withoutDestinations, HtmlToTextUtils.HtmlToText(html, includeLinkDestinations: false));
        }

        [Theory]
        [InlineData("<ul><li><a href=\"https://example.com\">report</a></li></ul>", "- report")]
        [InlineData("<ol><li><ul><li><a href=\"https://example.com\">report</a></li></ul></li></ol>", "1. - report")]
        [InlineData("<dl><dt>Term</dt><dd><a href=\"https://example.com\">report</a></dd></dl>", "Term\r\n  report")]
        [InlineData(
            "<table><tbody><tr><td><a href=\"https://example.com\">report</a></td></tr></tbody></table>",
            "report"
        )]
        public void PropagatesOptionIntoNestedRenderers(string html, string expected)
        {
            Assert.Contains("(https://example.com)", HtmlToTextUtils.HtmlToText(html));
            Assert.Equal(expected, HtmlToTextUtils.HtmlToText(html, includeLinkDestinations: false));
        }
    }
}
