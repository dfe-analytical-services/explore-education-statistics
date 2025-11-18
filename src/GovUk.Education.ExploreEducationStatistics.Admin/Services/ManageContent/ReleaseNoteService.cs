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

public class ReleaseNoteService : IReleaseNoteService
{
    private readonly IMapper _mapper;
    private readonly ContentDbContext _context;
    private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
    private readonly IUserService _userService;

    public ReleaseNoteService(
        IMapper mapper,
        ContentDbContext context,
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IUserService userService
    )
    {
        _mapper = mapper;
        _context = context;
        _persistenceHelper = persistenceHelper;
        _userService = userService;
    }

    public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> AddReleaseNote(
        Guid releaseVersionId,
        ReleaseNoteSaveRequest saveRequest
    )
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateReleaseVersionForUpdates)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                _context.Update.Add(
                    new Update
                    {
                        // TODO EES-6490 Convert 'On' from DateTime to DateTimeOffset
                        On = saveRequest.On ?? DateTime.Now,
                        Reason = saveRequest.Reason,
                        ReleaseVersionId = releaseVersion.Id,
                        Created = DateTime.UtcNow,
                        CreatedById = _userService.GetUserId(),
                    }
                );

                await _context.SaveChangesAsync();
                return GetReleaseNoteViewModels(releaseVersion.Id);
            });
    }

    public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> UpdateReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId,
        ReleaseNoteSaveRequest saveRequest
    )
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateReleaseVersionForUpdates)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var releaseNote = releaseVersion.Updates.First(note => note.Id == releaseNoteId);

                if (releaseNote == null)
                {
                    return NotFound<List<ReleaseNoteViewModel>>();
                }

                releaseNote.On = saveRequest.On ?? DateTime.Now;
                releaseNote.Reason = saveRequest.Reason;

                _context.Update.Update(releaseNote);
                await _context.SaveChangesAsync();
                return GetReleaseNoteViewModels(releaseVersion.Id);
            });
    }

    public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> DeleteReleaseNote(
        Guid releaseVersionId,
        Guid releaseNoteId
    )
    {
        return _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, HydrateReleaseVersionForUpdates)
            .OnSuccess(_userService.CheckCanUpdateReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                var releaseNote = releaseVersion.Updates.First(note => note.Id == releaseNoteId);

                if (releaseNote == null)
                {
                    return NotFound<List<ReleaseNoteViewModel>>();
                }

                _context.Update.Remove(releaseNote);
                await _context.SaveChangesAsync();
                return GetReleaseNoteViewModels(releaseVersion.Id);
            });
    }

    private List<ReleaseNoteViewModel> GetReleaseNoteViewModels(Guid releaseVersionId)
    {
        var releaseNotes = _context
            .Update.AsQueryable()
            .Where(update => update.ReleaseVersionId == releaseVersionId)
            .OrderByDescending(update => update.On);
        return _mapper.Map<List<ReleaseNoteViewModel>>(releaseNotes);
    }

    private static IQueryable<ReleaseVersion> HydrateReleaseVersionForUpdates(IQueryable<ReleaseVersion> values)
    {
        return values.Include(rv => rv.Updates);
    }
}
