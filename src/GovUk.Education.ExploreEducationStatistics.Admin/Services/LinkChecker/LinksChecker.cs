#nullable enable
using System.Collections.Concurrent;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.LinkChecker;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.LinkChecker;

/// <summary>
/// This service is used to test the links in the content blocks of a release to see if they return 200 status or if they are bogus.
/// This is needed to validate links populated by analysts prior to release page redesign.
/// </summary>
/// <param name="httpClient"></param>
public class LinksChecker(HttpClient httpClient) : ILinksChecker
{
    public async Task<List<LinkDetails>> ExtractReleaseLinksAsync(
        ContentDbContext context,
        CancellationToken cancellationToken = default
    )
    {
        var releaseContentDetails = new List<LinkDetails>();
        Console.WriteLine("Identifying latest published versions and extracting links...");

        var latestVersions = context
            .ReleaseVersions.Where(rv => !rv.SoftDeleted && rv.Published != null)
            .GroupBy(rv => rv.ReleaseId)
            .Select(g => new { ReleaseId = g.Key, MaxVersion = g.Max(rv => rv.Version) });

        var latestPublishedVersionIds = context
            .ReleaseVersions.Where(rv => !rv.SoftDeleted)
            .Join(
                latestVersions,
                rv => new { rv.ReleaseId, rv.Version },
                lv => new { lv.ReleaseId, Version = lv.MaxVersion },
                (rv, lv) => rv.Id
            );

        var query = context
            .HtmlBlocks.AsNoTracking()
            .Where(cb => latestPublishedVersionIds.Contains(cb.ReleaseVersionId))
            .Where(cb => !string.IsNullOrEmpty(cb.Body) && cb.Body.Contains("find-statistics/"))
            .Select(cb => new ContentBodyDetails(
                cb.Body,
                cb.ReleaseVersion.Release.Publication.Title,
                cb.ContentSection,
                cb.ReleaseVersion.Release.Slug,
                cb.ReleaseVersion.Release.Publication.Slug
            ));

        await foreach (var row in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (string.IsNullOrWhiteSpace(row.Body))
                continue;

            var doc = new HtmlDocument();
            doc.LoadHtml(row.Body);

            var nodes = doc.DocumentNode.SelectNodes("//a[@href]");

            foreach (var node in nodes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var href = node.GetAttributeValue("href", "");
                var linkText = HtmlEntity.DeEntitize(node.InnerText).Trim();

                releaseContentDetails.Add(LinkDetails.FromContentBodyDetails(row, href, linkText));
            }
        }

        return releaseContentDetails;
    }

    public async Task<List<LinksCsvItem>> TestReleaseLinksAsync(
        List<LinkDetails> contentDetails,
        CancellationToken cancellationToken = default
    )
    {
        var options = new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = cancellationToken }; // Only 3 active workers
        var results = new ConcurrentBag<LinkDetailsStatusCode>();

        await Parallel.ForEachAsync(
            contentDetails,
            options,
            async (contentDetail, token) =>
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Head, contentDetail.Url); // only returns message-headers in the response
                    using var response = await httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        token
                    ); // return as soon as the response headers have been fully read.
                    var result = new LinkDetailsStatusCode((int)response.StatusCode, contentDetail);
                    results.Add(result);
                }
                catch (HttpRequestException)
                {
                    var result = new LinkDetailsStatusCode(0, contentDetail);
                    results.Add(result);
                } // Network/DNS issues
                catch (TaskCanceledException)
                {
                    var result = new LinkDetailsStatusCode(408, contentDetail);
                    results.Add(result);
                } // Timeout
            }
        );

        return results.Select(cd => cd.ToLinksCsvItem()).ToList();
    }
}
