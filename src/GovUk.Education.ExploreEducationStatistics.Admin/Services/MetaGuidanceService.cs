using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

                    if (string.IsNullOrEmpty(content))
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
            return await _metaGuidanceSubjectService.GetSubjects(release.Id)
                .OnSuccess(subjects => new MetaGuidanceViewModel
                {
                    Id = release.Id,
                    Content = release.MetaGuidance ?? "",
                    Subjects = subjects
                });
        }
    }
}