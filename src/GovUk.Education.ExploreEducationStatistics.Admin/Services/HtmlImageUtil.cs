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
        public static List<Guid> GetImages(string htmlContent)
        {
            if (htmlContent.IsNullOrEmpty())
            {
                return new List<Guid>();
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            htmlDoc.OptionEmptyCollection = true;

            var imageApiUrlRegex = new Regex(@"^/api/methodologies/{methodologyId}/images/(.+)$");

            var imageIds = htmlDoc.DocumentNode.SelectNodes("//img")
                .Select(node => node.Attributes["src"].Value)
                .Select(srcVal => imageApiUrlRegex.Match(srcVal))
                .Where(match => match.Success)
                .Select(match => match.Groups[1].Value)
                .ToList();

            // Convert to Guids, removing any that are malformed
            return imageIds
                .Select(id => Guid.TryParse(id, out var idAsGuid) ? idAsGuid : (Guid?) null)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();
        }
    }
}
