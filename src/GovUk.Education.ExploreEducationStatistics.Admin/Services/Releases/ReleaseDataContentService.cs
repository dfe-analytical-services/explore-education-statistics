using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases;

public class ReleaseDataContentService(ContentDbContext contentDbContext, IUserService userService)
    : IReleaseDataContentService
{
    public async Task<Either<ActionResult, ReleaseDataContentDto>> GetReleaseDataContent(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    ) =>
        await GetReleaseVersion(releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
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

    private Task<Either<ActionResult, ReleaseVersion>> GetReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .ReleaseVersions.AsNoTracking()
            .Include(rv => rv.Content.Where(cs => cs.Type == ContentSectionType.RelatedDashboards))
                .ThenInclude(cs => cs.Content)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken);

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
            .Where(rf => rf.File.ReplacingId == null)
            .Join(
                contentDbContext.DataImports.Where(di => di.Status == DataImportStatus.COMPLETE),
                rf => rf.FileId,
                di => di.FileId,
                (rf, di) => rf
            )
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
