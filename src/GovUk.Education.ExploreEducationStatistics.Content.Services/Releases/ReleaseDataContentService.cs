using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Predicates;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public class ReleaseDataContentService(ContentDbContext contentDbContext) : IReleaseDataContentService
{
    public async Task<Either<ActionResult, ReleaseDataContentDto>> GetReleaseDataContent(
        string publicationSlug,
        string releaseSlug,
        CancellationToken cancellationToken = default
    ) =>
        await GetPublicationBySlug(publicationSlug, cancellationToken)
            .OnSuccess(publication =>
                GetLatestPublishedReleaseVersionWithContentByReleaseSlug(
                    publication: publication,
                    releaseSlug: releaseSlug,
                    cancellationToken: cancellationToken
                )
            )
            .OnSuccess(async releaseVersion =>
            {
                var dataSets = await GetDataSets(releaseVersion, cancellationToken);
                var featuredTables = await GetFeaturedTables(releaseVersion, cancellationToken);
                var supportingFiles = await GetSupportingFiles(releaseVersion, cancellationToken);
                return ReleaseDataContentDto.FromReleaseVersion(
                    releaseVersion: releaseVersion,
                    dataSets: dataSets,
                    featuredTables: featuredTables,
                    supportingFiles: supportingFiles
                );
            });

    private Task<Either<ActionResult, Publication>> GetPublicationBySlug(
        string publicationSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .Publications.AsNoTracking()
            .WhereHasPublishedRelease()
            .SingleOrNotFoundAsync(p => p.Slug == publicationSlug, cancellationToken);

    private Task<Either<ActionResult, ReleaseVersion>> GetLatestPublishedReleaseVersionWithContentByReleaseSlug(
        Publication publication,
        string releaseSlug,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Include(rv => rv.Content.Where(cs => cs.Type == ContentSectionType.RelatedDashboards))
                .ThenInclude(cs => cs.Content)
            .LatestReleaseVersions(publication.Id, releaseSlug, publishedOnly: true)
            .SingleOrNotFoundAsync(cancellationToken);

    private async Task<ReleaseDataContentDataSetDto[]> GetDataSets(
        ReleaseVersion releaseVersion,
        CancellationToken cancellationToken
    )
    {
        var releaseFiles = await contentDbContext
            .ReleaseFiles.AsNoTracking()
            .Include(rf => rf.File)
                .ThenInclude(f => f.DataSetFileVersionGeographicLevels)
            .Where(rf => rf.ReleaseVersionId == releaseVersion.Id)
            .Where(rf => rf.File.Type == FileType.Data)
            .OrderBy(rf => rf.Order)
            .ToArrayAsync(cancellationToken);
        return releaseFiles.Select(ReleaseDataContentDataSetDto.FromReleaseFile).ToArray();
    }

    private async Task<ReleaseDataContentFeaturedTableDto[]> GetFeaturedTables(
        ReleaseVersion releaseVersion,
        CancellationToken cancellationToken
    )
    {
        var featuredTables = await contentDbContext
            .FeaturedTables.AsNoTracking()
            .Where(ft => ft.ReleaseVersionId == releaseVersion.Id)
            .OrderBy(ft => ft.Order)
            .ToArrayAsync(cancellationToken);
        return featuredTables.Select(ReleaseDataContentFeaturedTableDto.FromFeaturedTable).ToArray();
    }

    private async Task<ReleaseDataContentSupportingFileDto[]> GetSupportingFiles(
        ReleaseVersion releaseVersion,
        CancellationToken cancellationToken
    )
    {
        var releaseFiles = await contentDbContext
            .ReleaseFiles.AsNoTracking()
            .Include(rf => rf.File)
            .Where(rf => rf.ReleaseVersionId == releaseVersion.Id)
            .Where(rf => rf.File.Type == FileType.Ancillary)
            .OrderBy(rf => rf.Order)
            .ToArrayAsync(cancellationToken);
        return releaseFiles.Select(ReleaseDataContentSupportingFileDto.FromReleaseFile).ToArray();
    }
}
