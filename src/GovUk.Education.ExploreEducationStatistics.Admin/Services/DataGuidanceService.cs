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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class DataGuidanceService : IDataGuidanceService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IDataGuidanceSubjectService _dataGuidanceSubjectService;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IUserService _userService;
        private readonly IReleaseDataFileRepository _fileRepository;

        public DataGuidanceService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IDataGuidanceSubjectService dataGuidanceSubjectService,
            StatisticsDbContext statisticsDbContext,
            IUserService userService,
            IReleaseDataFileRepository fileRepository)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _dataGuidanceSubjectService = dataGuidanceSubjectService;
            _statisticsDbContext = statisticsDbContext;
            _userService = userService;
            _fileRepository = fileRepository;
        }

        public async Task<Either<ActionResult, DataGuidanceViewModel>> Get(Guid releaseId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanViewRelease(release))
                .OnSuccess(BuildViewModel);
        }

        public async Task<Either<ActionResult, DataGuidanceViewModel>> Update(
            Guid releaseId,
            DataGuidanceUpdateViewModel request)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccessDo(async release =>
                {
                    _contentDbContext.Update(release);
                    release.DataGuidance = request.Content;
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
                    if (await _fileRepository.HasAnyDataFiles(release.Id))
                    {
                        if (string.IsNullOrWhiteSpace(release.DataGuidance))
                        {
                            return ValidationActionResult(ValidationErrorMessages.PublicDataGuidanceRequired);
                        }

                        return await _dataGuidanceSubjectService.Validate(releaseId)
                            .OnSuccess(valid => valid
                                ? (Either<ActionResult, Unit>) Unit.Instance
                                : ValidationActionResult(ValidationErrorMessages.PublicDataGuidanceRequired));
                    }

                    return Unit.Instance;
                });
        }

        private async Task UpdateSubjects(
            Guid releaseId,
            List<DataGuidanceUpdateSubjectViewModel> subjects)
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
                .AsQueryable()
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

                    releaseSubject.DataGuidance = content;
                    _statisticsDbContext.Update(releaseSubject);
                });

            await _statisticsDbContext.SaveChangesAsync();
        }

        private async Task<Either<ActionResult, DataGuidanceViewModel>> BuildViewModel(Release release)
        {
            var subjectIds = (await _fileRepository.ListDataFiles(release.Id))
                .Where(f => f.SubjectId.HasValue)
                .Select(f => f.SubjectId.Value)
                .ToList();

            return await _dataGuidanceSubjectService.GetSubjects(release.Id, subjectIds)
                .OnSuccess(subjects => new DataGuidanceViewModel
                {
                    Id = release.Id,
                    Content = release.DataGuidance ?? "",
                    Subjects = subjects
                });
        }
    }
}
