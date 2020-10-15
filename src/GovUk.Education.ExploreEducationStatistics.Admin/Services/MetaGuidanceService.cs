using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class MetaGuidanceService : IMetaGuidanceService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly IMetaGuidanceSubjectService _metaGuidanceSubjectService;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IPersistenceHelper<StatisticsDbContext> _statisticsPersistenceHelper;
        private readonly IUserService _userService;

        public MetaGuidanceService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            IMetaGuidanceSubjectService metaGuidanceSubjectService,
            StatisticsDbContext statisticsDbContext,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _metaGuidanceSubjectService = metaGuidanceSubjectService;
            _statisticsDbContext = statisticsDbContext;
            _statisticsPersistenceHelper = statisticsPersistenceHelper;
            _userService = userService;
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> Get(Guid releaseId)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanViewRelease(release))
                .OnSuccess(BuildViewModel);
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> UpdateRelease(Guid releaseId,
            MetaGuidanceUpdateReleaseViewModel request)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccessDo(async release =>
                {
                    _contentDbContext.Update(release);
                    release.MetaGuidance = request.Content;
                    await _contentDbContext.SaveChangesAsync();
                })
                .OnSuccess(BuildViewModel);
        }

        public async Task<Either<ActionResult, MetaGuidanceViewModel>> UpdateSubject(Guid releaseId,
            Guid subjectId,
            MetaGuidanceUpdateSubjectViewModel request)
        {
            return await _contentPersistenceHelper.CheckEntityExists<Release>(releaseId)
                .OnSuccessDo(release => _userService.CheckCanUpdateRelease(release))
                .OnSuccess(release =>
                {
                    return _statisticsPersistenceHelper.CheckEntityExists<ReleaseSubject>(
                            releaseSubjects => releaseSubjects
                                .Where(releaseSubject => releaseSubject.ReleaseId == releaseId
                                                         && releaseSubject.SubjectId == subjectId))
                        .OnSuccessDo(async releaseSubject =>
                        {
                            _statisticsDbContext.Update(releaseSubject);
                            releaseSubject.MetaGuidance = request.Content;
                            await _statisticsDbContext.SaveChangesAsync();
                        })
                        .OnSuccess(() => BuildViewModel(release));
                });
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