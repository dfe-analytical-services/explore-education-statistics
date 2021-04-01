using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using HtmlAgilityPack;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public static class HtmlImageUtil
    {
        public static List<Guid> GetMethodologyImages(string htmlContent)
        {
            var idCapturingRegex = new Regex(@"^/api/methodologies/{methodologyId}/images/(.+)$");
            return GetImages(htmlContent, idCapturingRegex);
        }

        public static List<Guid> GetReleaseImages(string htmlContent)
        {
            var idCapturingRegex = new Regex(@"^/api/releases/{releaseId}/images/(.+)$");
            return GetImages(htmlContent, idCapturingRegex);
        }

        private static List<Guid> GetImages(string htmlContent, Regex idCapturingRegex)
        {
            if (htmlContent.IsNullOrEmpty())
            {
                return new List<Guid>();
            }

            var htmlDocument = ParseContentAsHtml(htmlContent);
            var imageIds = htmlDocument.DocumentNode.SelectNodes("//img")
                .Select(node => node.Attributes["src"].Value)
                .Select(srcVal => idCapturingRegex.Match(srcVal))
                .Where(match => match.Success)
                .Select(match => match.Groups[1].Value)
                .ToList();

            // Convert to Guids, removing any that are malformed
            return ParseIdsAsGuids(imageIds);
        }

        private static HtmlDocument ParseContentAsHtml(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            htmlDoc.OptionEmptyCollection = true;
            return htmlDoc;
        }

        private static List<Guid> ParseIdsAsGuids(IEnumerable<string> ids)
        {
            return ids
                .Select(id => Guid.TryParse(id, out var idAsGuid) ? idAsGuid : (Guid?) null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();
        }
    }
}
