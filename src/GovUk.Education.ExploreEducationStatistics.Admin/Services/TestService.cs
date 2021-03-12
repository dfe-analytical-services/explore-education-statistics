using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using HtmlAgilityPack;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    // TODO EES-1991 Rename this or merge it into another service
    public class TestService : ITestService
    {
        public List<Guid> GetImages(string content)
        {
            if (content.IsNullOrEmpty())
            {
                return new List<Guid>();
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(content);
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
