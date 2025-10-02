#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

internal class DataSetCandidateService(ContentDbContext contentDbContext, IUserService userService)
    : IDataSetCandidateService
{
    public async Task<Either<ActionResult, IReadOnlyList<DataSetCandidateViewModel>>> ListCandidates(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await CheckReleaseVersionExists(releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(async () => await DoListCandidates(releaseVersionId, cancellationToken));
    }

    private async Task<Either<ActionResult, ReleaseVersion>> CheckReleaseVersionExists(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .ReleaseVersions.AsNoTracking()
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, IReadOnlyList<DataSetCandidateViewModel>>> DoListCandidates(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .ReleaseFiles.AsNoTracking()
            .Where(rf => rf.ReleaseVersionId == releaseVersionId)
            .Where(rf => rf.File.Type == FileType.Data)
            .Where(rf => rf.PublicApiDataSetId == null)
            .Where(rf => rf.File.ReplacedById == null)
            .Where(rf => rf.File.ReplacingId == null)
            .Join(
                contentDbContext.DataImports,
                rf => rf.FileId,
                di => di.FileId,
                (rf, di) => new { ReleaseFile = rf, DataImport = di }
            )
            .Where(tuple => tuple.DataImport.Status == DataImportStatus.COMPLETE)
            .Select(tuple => tuple.ReleaseFile)
            .Select(rf => new DataSetCandidateViewModel { ReleaseFileId = rf.Id, Title = rf.Name! })
            .OrderBy(rf => rf.Title)
            .ToListAsync(cancellationToken: cancellationToken);
    }
}
