#nullable enable
using System.Threading.Tasks;
using AngleSharp;
using GovUk.Education.ExploreEducationStatistics.Common.Utils.Html;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public static class HtmlToTextUtils
    {
        public static async Task<string> HtmlToText(string html)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);

            var document = await context.OpenAsync(req => req.Content(html));

            var converter = new HtmlToTextConverter();
            return converter.Convert(document.Body);
        }
    }
}