using AngleSharp.Html.Parser;
using GovUk.Education.ExploreEducationStatistics.Common.Utils.Html;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public static class HtmlToTextUtils
{
    public static string HtmlToText(string html)
    {
        var parser = new HtmlParser();
        var document = parser.ParseDocument("");

        var converter = new HtmlToTextConverter();
        return converter.Convert(parser.ParseFragment(html, document.Body!));
    }
}
