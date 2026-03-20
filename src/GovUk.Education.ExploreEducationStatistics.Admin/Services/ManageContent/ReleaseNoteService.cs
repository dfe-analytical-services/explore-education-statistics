#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
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
    public async Task<Either<ActionResult, List<ReleaseNoteViewModel>>> CreateReleaseNote(
        Guid releaseVersionId,
        ReleaseNoteCreateRequest createRequest,
        CancellationToken cancellationToken = default
    ) =>
        await contentDbContext
            .ReleaseVersions.SingleOrNotFoundAsync(
                rv => rv.Id == releaseVersionId,
                cancellationToken: cancellationToken
            )
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                contentDbContext.Update.Add(
                    new Update
                    {
                        // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
                        On = DateTimeOffset.UtcNow.GetUkStartOfDayUtc().UtcDateTime,
                        Reason = createRequest.Reason,
                        ReleaseVersionId = releaseVersion.Id,
                        CreatedById = userService.GetUserId(),
                    }
                );
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return await GetReleaseNoteViewModels(releaseVersion.Id, cancellationToken);
            });

    public async Task<Either<ActionResult, List<ReleaseNoteViewModel>>> UpdateReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        ReleaseNoteUpdateRequest updateRequest,
        CancellationToken cancellationToken = default
    ) =>
        await contentDbContext
            .ReleaseVersions.Include(rv => rv.Updates)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var releaseNote = releaseVersion.Updates.Single(u => u.Id == releaseNoteId);
                // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
                releaseNote.On = updateRequest.On.GetUkStartOfDayUtc().UtcDateTime;
                releaseNote.Reason = updateRequest.Reason;
                await contentDbContext.SaveChangesAsync(cancellationToken);

                return await GetReleaseNoteViewModels(releaseVersion.Id, cancellationToken);
            });

    public async Task<Either<ActionResult, List<ReleaseNoteViewModel>>> DeleteReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        CancellationToken cancellationToken = default
    ) =>
        await contentDbContext
            .ReleaseVersions.Include(rv => rv.Updates)
            .SingleOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken: cancellationToken)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var releaseNote = releaseVersion.Updates.Single(u => u.Id == releaseNoteId);
                releaseVersion.Updates.Remove(releaseNote);

                await contentDbContext.SaveChangesAsync(cancellationToken);

                return await GetReleaseNoteViewModels(releaseVersion.Id, cancellationToken);
            });

    private async Task<List<ReleaseNoteViewModel>> GetReleaseNoteViewModels(
        Guid releaseVersionId,
        CancellationToken cancellationToken
    ) =>
        await contentDbContext
            .Update.Where(u => u.ReleaseVersionId == releaseVersionId)
            .OrderByDescending(u => u.On)
            .Select(u => ReleaseNoteViewModel.FromUpdate(u))
            .ToListAsync(cancellationToken: cancellationToken);
}
