using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent
{
    public class ReleaseNoteService : IReleaseNoteService
    {
        private readonly IMapper _mapper;
        private readonly ContentDbContext _context;
        private readonly PersistenceHelper<Release, Guid> _releaseHelper;
        private readonly PersistenceHelper<Update, Guid> _updateHelper;

        public ReleaseNoteService(IMapper mapper, ContentDbContext context)
        {
            _mapper = mapper;
            _context = context;
            _releaseHelper =
                new PersistenceHelper<Release, Guid>(_context, context.Releases,
                    ValidationErrorMessages.ReleaseNotFound);
            _updateHelper = new PersistenceHelper<Update, Guid>(_context, context.Update,
                ValidationErrorMessages.ReleaseNoteNotFound);
        }

        public Task<Either<ValidationResult, List<ReleaseNoteViewModel>>> AddReleaseNoteAsync(Guid releaseId,
            CreateOrUpdateReleaseNoteRequest request)
        {
            return _releaseHelper.CheckEntityExists(releaseId, async release =>
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

        public Task<Either<ValidationResult, List<ReleaseNoteViewModel>>> UpdateReleaseNoteAsync(Guid releaseId,
            Guid releaseNoteId, CreateOrUpdateReleaseNoteRequest request)
        {
            return _releaseHelper.CheckEntityExists(releaseId, release =>
            {
                return _updateHelper.CheckEntityExists(releaseNoteId, async releaseNote =>
                {
                    releaseNote.On = request.On ?? DateTime.Now;
                    releaseNote.Reason = request.Reason;

                    _context.Update.Update(releaseNote);
                    await _context.SaveChangesAsync();
                    return GetReleaseNoteViewModels(release.Id);
                });
            });
        }

        public Task<Either<ValidationResult, List<ReleaseNoteViewModel>>> DeleteReleaseNoteAsync(Guid releaseId,
            Guid releaseNoteId)
        {
            return _releaseHelper.CheckEntityExists(releaseId, release =>
            {
                return _updateHelper.CheckEntityExists(releaseNoteId, async releaseNote =>
                {
                    _context.Update.Remove(releaseNote);
                    await _context.SaveChangesAsync();
                    return GetReleaseNoteViewModels(release.Id);
                });
            });
        }

        private List<ReleaseNoteViewModel> GetReleaseNoteViewModels(Guid releaseId)
        {
            var releaseNotes = _context.Update
                .Where(update => update.ReleaseId == releaseId)
                .OrderByDescending(update => update.On);
            return _mapper.Map<List<ReleaseNoteViewModel>>(releaseNotes);
        }
    }
}