using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ReleaseNoteService : IReleaseNoteService
    {
        private readonly IMapper _mapper;
        private readonly ContentDbContext _context;
        private readonly IPersistenceHelper<Release, Guid> _releaseHelper;
        private readonly IUserService _userService;

        public ReleaseNoteService(
            IMapper mapper, 
            ContentDbContext context, 
            IPersistenceHelper<Release, Guid> releaseHelper, 
            IUserService userService)
        {
            _mapper = mapper;
            _context = context;
            _releaseHelper = releaseHelper;
            _userService = userService;
        }

        public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> AddReleaseNoteAsync(Guid releaseId,
            CreateOrUpdateReleaseNoteRequest request)
        {
            return _releaseHelper
                .CheckEntityExists(releaseId, HydrateReleaseForUpdates)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    _context.Update.Add(new Update
                    {
                        On = request.On ?? DateTime.Now,
                        Reason = request.Reason,
                        ReleaseId = release.Id
                    });

                    await _context.SaveChangesAsync();
                    return GetReleaseNoteViewModels(release.Id);
                });
        }

        public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> UpdateReleaseNoteAsync(Guid releaseId,
            Guid releaseNoteId, CreateOrUpdateReleaseNoteRequest request)
        {
            return _releaseHelper
                .CheckEntityExists(releaseId, HydrateReleaseForUpdates)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var releaseNote = release
                        .Updates
                        .First(note => note.Id == releaseNoteId);

                    if (releaseNote == null)
                    {
                        return NotFound<List<ReleaseNoteViewModel>>();
                    }

                    releaseNote.On = request.On ?? DateTime.Now;
                    releaseNote.Reason = request.Reason;

                    _context.Update.Update(releaseNote);
                    await _context.SaveChangesAsync();
                    return GetReleaseNoteViewModels(release.Id);
                });
        }

        public Task<Either<ActionResult, List<ReleaseNoteViewModel>>> DeleteReleaseNoteAsync(Guid releaseId,
            Guid releaseNoteId)
        {
            return _releaseHelper
                .CheckEntityExists(releaseId, HydrateReleaseForUpdates)
                .OnSuccess(_userService.CheckCanUpdateRelease)
                .OnSuccess(async release =>
                {
                    var releaseNote = release
                        .Updates
                        .First(note => note.Id == releaseNoteId);

                    if (releaseNote == null)
                    {
                        return NotFound<List<ReleaseNoteViewModel>>();
                    }
                    
                    _context.Update.Remove(releaseNote);
                    await _context.SaveChangesAsync();
                    return GetReleaseNoteViewModels(release.Id);
                });
        }

        private List<ReleaseNoteViewModel> GetReleaseNoteViewModels(Guid releaseId)
        {
            var releaseNotes = _context.Update
                .Where(update => update.ReleaseId == releaseId)
                .OrderByDescending(update => update.On);
            return _mapper.Map<List<ReleaseNoteViewModel>>(releaseNotes);
        }
        
        public static IQueryable<Release> HydrateReleaseForUpdates(IQueryable<Release> values)
        {
            return values.Include(r => r.Updates);
        }
    }
}