using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class AmendmentMigrationService : IAmendmentMigrationService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IUserService _userService;
        private readonly IReleaseFilesService _releaseFilesService;

        public AmendmentMigrationService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IUserService userService,
            IReleaseFilesService releaseFilesService)
        {
            _contentDbContext = contentDbContext;
            _persistenceHelper = persistenceHelper;
            _userService = userService;
            _persistenceHelper = persistenceHelper;
            _releaseFilesService = releaseFilesService;
        }

        public async Task<Either<ActionResult, bool>> FixMissingSubjectId()
        {
            var releaseIds = _contentDbContext
                .Releases
                .Where(r => r.Id == r.PreviousVersionId)
                .ToList()
                .Select(r => r.Id);
            
            foreach (var releaseId in releaseIds)
            {
                var result = await FixMissingSubjectIdForRelease(releaseId);

                if (result.IsLeft)
                {
                    return false;
                }
            }

            return true;
        }

        private Task<Either<ActionResult, Release>> FixMissingSubjectIdForRelease(Guid releaseId)
        {
            return _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanRunReleaseMigrations)
                .OnSuccess(UpdateSubjectId);
        }

        private async Task<Either<ActionResult, Release>> UpdateSubjectId(Release release)
        {
            var result = _releaseFilesService.ListFilesAsync(release.Id, ReleaseFileTypes.Data).Result;
            var files = result.Right;

            foreach (var file in files)
            {
                var dataFileRef = _contentDbContext.ReleaseFileReferences
                    .Where(rfr => rfr.ReleaseId == release.Id && rfr.ReleaseFileType == ReleaseFileTypes.Data)
                    .ToList()
                    .FirstOrDefault(rfr => rfr.Filename == file.FileName);

                var metaFileRef = _contentDbContext.ReleaseFileReferences
                    .Where(rfr => rfr.ReleaseId == release.Id && rfr.ReleaseFileType == ReleaseFileTypes.Metadata)
                    .ToList()
                    .FirstOrDefault(rfr => rfr.Filename == file.MetaFileName);
                
                if (dataFileRef != null && metaFileRef != null)
                {
                    metaFileRef.SubjectId = dataFileRef.SubjectId;
                    _contentDbContext.ReleaseFileReferences.Update(metaFileRef);
                }
            }
            
            await _contentDbContext.SaveChangesAsync();

            return release;
        }
    }
}