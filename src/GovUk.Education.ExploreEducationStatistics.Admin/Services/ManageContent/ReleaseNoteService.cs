#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;

public class ReleaseNoteService(
    IMapper mapper,
    ContentDbContext context,
    IPersistenceHelper<ContentDbContext> persistenceHelper,
    IUserService userService
) : IReleaseNoteService
{
    public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> AddReleaseNote(
        Guid releaseVersionId,
        ReleaseNoteSaveRequest saveRequest,
        CancellationToken cancellationToken = default
    ) =>
        persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateReleaseVersionForUpdates)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                context.Update.Add(
                    new Update
                    {
                        // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
                        On = saveRequest.On ?? DateTime.Now,
                        Reason = saveRequest.Reason,
                        ReleaseVersionId = releaseVersion.Id,
                        Created = DateTime.UtcNow,
                        CreatedById = userService.GetUserId(),
                    }
                );

                await context.SaveChangesAsync(cancellationToken);
                return GetReleaseNoteViewModels(releaseVersion.Id);
            });

    public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> UpdateReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        ReleaseNoteSaveRequest saveRequest,
        CancellationToken cancellationToken = default
    ) =>
        persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateReleaseVersionForUpdates)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var releaseNote = releaseVersion.Updates.First(note => note.Id == releaseNoteId);

                if (releaseNote == null)
                {
                    return NotFound<List<ReleaseNoteViewModel>>();
                }

                releaseNote.On = saveRequest.On ?? DateTime.Now;
                releaseNote.Reason = saveRequest.Reason;

                context.Update.Update(releaseNote);
                await context.SaveChangesAsync(cancellationToken);
                return GetReleaseNoteViewModels(releaseVersion.Id);
            });

    public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> DeleteReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        CancellationToken cancellationToken = default
    ) =>
        persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateReleaseVersionForUpdates)
            .OnSuccess(userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var releaseNote = releaseVersion.Updates.First(note => note.Id == releaseNoteId);

                if (releaseNote == null)
                {
                    return NotFound<List<ReleaseNoteViewModel>>();
                }

                context.Update.Remove(releaseNote);
                await context.SaveChangesAsync(cancellationToken);
                return GetReleaseNoteViewModels(releaseVersion.Id);
            });

    private List<ReleaseNoteViewModel> GetReleaseNoteViewModels(Guid releaseVersionId)
    {
        var releaseNotes = context
            .Update.AsQueryable()
            .Where(update => update.ReleaseVersionId == releaseVersionId)
            .OrderByDescending(update => update.On);
        return mapper.Map<List<ReleaseNoteViewModel>>(releaseNotes);
    }

    private static IQueryable<ReleaseVersion> HydrateReleaseVersionForUpdates(IQueryable<ReleaseVersion> values) =>
        values.Include(rv => rv.Updates);
}
