#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class ReleaseService(
    ContentDbContext contentDbContext,
    IUserService userService)
    : IReleaseService
{
    public async Task<Either<ActionResult, IReadOnlyList<ApiDataSetCandidateViewModel>>> GetApiDataSetCandidates(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await CheckReleaseVersionExists(releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanViewReleaseVersion)
            .OnSuccess(async () => await GetApiDataSetCandidates(releaseVersionId));
    }

    private async Task<Either<ActionResult, ReleaseVersion>> CheckReleaseVersionExists(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default)
    {
        return await contentDbContext.ReleaseVersions
            .AsNoTracking()
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken);
    }

    private async Task<Either<ActionResult, IReadOnlyList<ApiDataSetCandidateViewModel>>> GetApiDataSetCandidates(Guid releaseVersionId)
    {
        return await contentDbContext.ReleaseFiles
            .AsNoTracking()
            .Include(rf => rf.File)
            .Where(rf => rf.ReleaseVersionId == releaseVersionId)
            .Where(rf => rf.File.Type == FileType.Data)
            .Where(rf => rf.File.PublicDataSetVersionId == null)
            .Where(rf => rf.File.ReplacedById == null)
            .Where(rf => rf.File.ReplacingId == null)
            .Select(rf => new ApiDataSetCandidateViewModel
            {
                FileId = rf.FileId,
                Title = rf.Name,
            })
            .OrderBy(rf => rf.Title)
            .ToListAsync();
    }
}
