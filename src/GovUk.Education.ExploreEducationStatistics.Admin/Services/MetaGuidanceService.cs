using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class MetaGuidanceService : IMetaGuidanceService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IMetaGuidanceSubjectService _metaGuidanceSubjectService;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IUserService _userService;

        public MetaGuidanceService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IMetaGuidanceSubjectService metaGuidanceSubjectService,
            StatisticsDbContext statisticsDbContext,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _metaGuidanceSubjectService = metaGuidanceSubjectService;
            _statisticsDbContext = statisticsDbContext;
            _userService = userService;
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Get(Guid releaseId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanViewRelease(release))
                .OnSuccess(BuildViewModel);
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Update(
            Guid releaseId,
            MetaGuidanceUpdateViewModel request)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccessDo(async release =>
                {
                    _contentDbContext.Update(release);
                    release.MetaGuidance = request.Content;
                    await _contentDbContext.SaveChangesAsync();

                    await UpdateSubjects(releaseId, request.Subjects);
                })
                .OnSuccess(BuildViewModel);
        }

        public async Task<Either<ActionResult, Unit>> Validate(Guid releaseId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccess(async release =>
                {
                    if (await DataFilesExist(releaseId))
                    {
                        if (string.IsNullOrWhiteSpace(release.MetaGuidance))
                        {
                            return ValidationActionResult(ValidationErrorMessages.MetaGuidanceMustBePopulated);
                        }

                        return await _metaGuidanceSubjectService.Validate(releaseId)
                            .OnSuccess(valid => valid
                                ? (Either<ActionResult, Unit>) Unit.Instance
                                : ValidationActionResult(ValidationErrorMessages.MetaGuidanceMustBePopulated));
                    }

                    return Unit.Instance;
                });
        }

        private async Task UpdateSubjects(
            Guid releaseId,
            List<MetaGuidanceUpdateSubjectViewModel> subjects)
        {
            if (!subjects.Any())
            {
                return;
            }

            var contentById = subjects.ToDictionary(
                s => s.Id,
                s => s.Content
            );
            var subjectIds = subjects.Select(s => s.Id);

            var matchingSubjects = await _statisticsDbContext.ReleaseSubject
                .Where(
                    releaseSubject => releaseSubject.ReleaseId == releaseId
                                      && subjectIds.Contains(releaseSubject.SubjectId)
                )
                .ToListAsync();

            matchingSubjects.ForEach(
                releaseSubject =>
                {
                    var content = contentById.GetValueOrDefault(releaseSubject.SubjectId);

                    if (content.IsNullOrEmpty())
                    {
                        return;
                    }

                    releaseSubject.MetaGuidance = content;
                    _statisticsDbContext.Update(releaseSubject);
                });

            await _statisticsDbContext.SaveChangesAsync();
        }

        private async Task<Either<ActionResult, MetaGuidanceViewModel>> BuildViewModel(Release release)
        {
            var subjectIds = await GetDataFilesQueryable(release.Id)
                .Select(f => f.SubjectId.Value)
                .ToListAsync();

            return await _metaGuidanceSubjectService.GetSubjects(release.Id, subjectIds)
                .OnSuccess(subjects => new MetaGuidanceViewModel
                {
                    Id = release.Id,
                    Content = release.MetaGuidance ?? "",
                    Subjects = subjects
                });
        }

        private async Task<bool> DataFilesExist(Guid releaseId)
        {
            return await GetDataFilesQueryable(releaseId).AnyAsync();
        }

        private IQueryable<ReleaseFileReference> GetDataFilesQueryable(Guid releaseId)
        {
            return _contentDbContext
                .ReleaseFiles
                .Include(rf => rf.ReleaseFileReference)
                .Where(rf => rf.ReleaseId == releaseId
                             && rf.ReleaseFileReference.ReleaseFileType == ReleaseFileTypes.Data
                             && rf.ReleaseFileReference.ReplacingId == null
                             && rf.ReleaseFileReference.SubjectId.HasValue)
                .Select(rf => rf.ReleaseFileReference);
        }
    }
}