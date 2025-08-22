using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public class PublicationsSitemapService(ContentDbContext contentDbContext) : IPublicationsSitemapService
{
    public async Task<PublicationSitemapItemDto[]> GetSitemapItems(CancellationToken cancellationToken = default) =>
        await contentDbContext.Publications
            .Include(p => p.Releases)
            .ThenInclude(r => r.Versions)
            .Where(p => p.LatestPublishedReleaseVersionId.HasValue &&
                        (p.SupersededById == null || !p.SupersededBy!.LatestPublishedReleaseVersionId.HasValue))
            .Select(p => new PublicationSitemapItemDto
            {
                Slug = p.Slug,
                LastModified = p.Updated,
                Releases = GetUniqueReleaseVersionSitemapItems(p)
            })
            .ToArrayAsync(cancellationToken);

    private static ReleaseSitemapItemDto[] GetUniqueReleaseVersionSitemapItems(Publication publication) =>
        publication.Releases
            .SelectMany(r => r.Versions)
            .Where(rv => rv.Published != null) // r.Live cannot be translated by LINQ
            .OrderByDescending(rv => rv.Published)
            .GroupBy(rv => rv.Release)
            .Select(grouping => new ReleaseSitemapItemDto
            {
                Slug = grouping.Key.Slug,
                LastModified = grouping.First().Published
            })
            .ToArray();
}
