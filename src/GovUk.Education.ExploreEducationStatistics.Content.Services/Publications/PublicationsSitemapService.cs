using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public class PublicationsSitemapService(ContentDbContext contentDbContext) : IPublicationsSitemapService
{
    public async Task<PublicationSitemapPublicationDto[]> GetSitemapItems(CancellationToken cancellationToken = default)
    {
        // Fetch the latest published release versions for non-superseded publications
        var latestPublishedReleaseVersions = await GetLatestPublishedReleaseVersions(cancellationToken);

        // Group the latest published release versions by publication, and map to sitemap DTOs
        return latestPublishedReleaseVersions
            .GroupBy(rv => rv.Release.Publication.Id)
            .Select(grouping =>
            {
                // All release versions in the grouping share the same publication
                var publication = grouping.First().Release.Publication;

                // There is one release per latest published release version
                var releases = grouping
                    .OrderByDescending(rv => rv.Published)
                    .Select(PublicationSitemapReleaseDto.FromReleaseVersion)
                    .ToArray();

                return PublicationSitemapPublicationDto.FromPublication(publication, releases);
            })
            .OrderByDescending(p => p.Releases.Max(r => r.LastModified))
            .ToArray();
    }

    private Task<ReleaseVersion[]> GetLatestPublishedReleaseVersions(CancellationToken cancellationToken = default) =>
        contentDbContext.ReleaseVersions
            .Include(rv => rv.Release.Publication)
            .LatestReleaseVersions(publishedOnly: true)
            .Where(rv => rv.Release.Publication.SupersededBy == null
                         || rv.Release.Publication.SupersededBy.LatestPublishedReleaseVersionId == null)
            .ToArrayAsync(cancellationToken);
}
