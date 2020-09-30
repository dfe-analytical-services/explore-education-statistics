using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReleaseMetaService : IReleaseMetaService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IUserService _userService;

        public ReleaseMetaService(ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            StatisticsDbContext statisticsDbContext,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _statisticsDbContext = statisticsDbContext;
            _userService = userService;
        }

        public async Task<Either<ActionResult, SubjectsMetaViewModel>> GetSubjects(Guid releaseId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release =>
                {
                    var files = _contentDbContext.ReleaseFiles
                        .Include(file => file.ReleaseFileReference)
                        .Where(file => file.ReleaseId == releaseId
                                       && file.ReleaseFileReference.ReleaseFileType == ReleaseFileTypes.Data)
                        .Select(file => file.ReleaseFileReference);

                    // Exclude files that are replacements in progress
                    var filesExcludingReplacements =
                        files.Where(file => !file.ReplacingId.HasValue);

                    var subjectIds = filesExcludingReplacements.Select(file => file.SubjectId).ToList();

                    var subjects = _statisticsDbContext.ReleaseSubject
                        .Include(subject => subject.Subject)
                        .Where(subject => subject.ReleaseId == releaseId
                                          && subjectIds.Contains(subject.SubjectId))
                        .Select(subject =>
                            new IdLabel(
                                subject.Subject.Id,
                                subject.Subject.Name))
                        .ToList();

                    return new SubjectsMetaViewModel
                    {
                        ReleaseId = releaseId,
                        Subjects = subjects
                    };
                });
        }
    }
}