#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class MethodologyNoteService : IMethodologyNoteService
    {
        private readonly IMapper _mapper;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IMethodologyNoteRepository _methodologyNoteRepository;
        private readonly IUserService _userService;

        public MethodologyNoteService(
            IMapper mapper,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMethodologyNoteRepository methodologyNoteRepository,
            IUserService userService)
        {
            _mapper = mapper;
            _persistenceHelper = persistenceHelper;
            _methodologyNoteRepository = methodologyNoteRepository;
            _userService = userService;
        }

        public Task<Either<ActionResult, MethodologyNoteViewModel>> AddNote(
            Guid methodologyVersionId,
            MethodologyNoteAddRequest request)
        {
            return _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId)
                .OnSuccess(_userService.CheckCanUpdateMethodologyVersion)
                .OnSuccess(async methodologyVersion =>
                {
                    var addedNote = await _methodologyNoteRepository.AddNote(
                        methodologyVersionId: methodologyVersion.Id,
                        createdByUserId: _userService.GetUserId(),
                        content: request.Content,
                        displayDate: request.DisplayDate ?? DateTime.Today.ToUniversalTime());

                    return BuildMethodologyNoteViewModel(addedNote);
                });
        }

        public async Task<Either<ActionResult, Unit>> DeleteNote(
            Guid methodologyVersionId,
            Guid methodologyNoteId)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyNote>(q =>
                    q.Include(n => n.MethodologyVersion)
                        .Where(
                            n => n.Id == methodologyNoteId
                                 && n.MethodologyVersionId == methodologyVersionId))
                .OnSuccessDo(methodologyNote =>
                    _userService.CheckCanUpdateMethodologyVersion(methodologyNote.MethodologyVersion))
                .OnSuccessVoid(async methodologyNote =>
                {
                    await _methodologyNoteRepository.DeleteNote(methodologyNoteId);
                });
        }

        public async Task<Either<ActionResult, MethodologyNoteViewModel>> UpdateNote(
            Guid methodologyVersionId,
            Guid methodologyNoteId,
            MethodologyNoteUpdateRequest request)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyNote>(q =>
                    q.Include(n => n.MethodologyVersion)
                        .Where(
                            n => n.Id == methodologyNoteId
                                 && n.MethodologyVersionId == methodologyVersionId))
                .OnSuccessDo(methodologyNote =>
                    _userService.CheckCanUpdateMethodologyVersion(methodologyNote.MethodologyVersion))
                .OnSuccess(async methodologyNote =>
                {
                    var updatedNote = await _methodologyNoteRepository.UpdateNote(
                        methodologyNoteId: methodologyNoteId,
                        updatedByUserId: _userService.GetUserId(),
                        content: request.Content,
                        displayDate: request.DisplayDate ?? DateTime.Today.ToUniversalTime()
                    );

                    return BuildMethodologyNoteViewModel(updatedNote);
                });
        }

        private MethodologyNoteViewModel BuildMethodologyNoteViewModel(MethodologyNote methodologyNote)
        {
            return _mapper.Map<MethodologyNoteViewModel>(methodologyNote);
        }
    }
}
