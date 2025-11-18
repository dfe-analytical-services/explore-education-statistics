#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class ReleaseNoteService(ContentDbContext contentDbContext, IUserService userService) : IReleaseNoteService
{
    public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> AddReleaseNote(
        Guid releaseVersionId,
        ReleaseNoteSaveRequest saveRequest,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                contentDbContext.Update.Add(
                    new Update
                    {
                        // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
                        On = saveRequest.On ?? DateTime.Now,
                        Reason = saveRequest.Reason,
                        ReleaseVersionId = releaseVersion.Id,
                        CreatedById = userService.GetUserId(),
                    }
                );

                await contentDbContext.SaveChangesAsync(cancellationToken);
                return await GetReleaseNoteViewModels(releaseVersion.Id, cancellationToken);
            });

    public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> UpdateReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        ReleaseNoteSaveRequest saveRequest,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await contentDbContext.Update.SingleOrNotFoundAsync(
                    u => u.Id == releaseNoteId && u.ReleaseVersionId == releaseVersion.Id,
                    cancellationToken
                )
            )
            .OnSuccess(async update =>
            {
                update.On = saveRequest.On ?? DateTime.Now;
                update.Reason = saveRequest.Reason;
                await contentDbContext.SaveChangesAsync(cancellationToken);
                return await GetReleaseNoteViewModels(releaseVersionId, cancellationToken);
            });

    public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> DeleteReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        CancellationToken cancellationToken = default
    ) =>
        contentDbContext
            .ReleaseVersions.SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
                await contentDbContext.Update.SingleOrNotFoundAsync(
                    u => u.Id == releaseNoteId && u.ReleaseVersionId == releaseVersion.Id,
                    cancellationToken
                )
            )
            .OnSuccess(async update =>
            {
                contentDbContext.Update.Remove(update);
                await contentDbContext.SaveChangesAsync(cancellationToken);
                return await GetReleaseNoteViewModels(releaseVersionId, cancellationToken);
            });

    private Task<List<ReleaseNoteViewModel>> GetReleaseNoteViewModels(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    ) =>
        contentDbContext
            .Update.AsNoTracking()
            .Where(u => u.ReleaseVersionId == releaseVersionId)
            .OrderByDescending(u => u.On)
            .Select(u => ReleaseNoteViewModel.FromModel(u))
            .ToListAsync(cancellationToken: cancellationToken);
}
