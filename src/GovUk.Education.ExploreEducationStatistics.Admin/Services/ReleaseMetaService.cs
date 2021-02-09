using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
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
        private readonly IImportStatusService _importStatusService;

        public ReleaseMetaService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            StatisticsDbContext statisticsDbContext,
            IUserService userService,
            IImportStatusService importStatusService)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _statisticsDbContext = statisticsDbContext;
            _userService = userService;
            _importStatusService = importStatusService;
        }

        public async Task<Either<ActionResult, ReleaseSubjectsMetaViewModel>> GetSubjectsMeta(Guid releaseId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(release =>
                {
                    var files = _contentDbContext.ReleaseFiles
                        .Include(file => file.File)
                        .Where(file => file.ReleaseId == releaseId
                                       && file.File.Type == FileType.Data)
                        .Select(file => file.File);

                    // Exclude files that are replacements in progress
                    var filesExcludingReplacements = files
                        .Where(file => !file.ReplacingId.HasValue)
                        .ToList();

                    var subjectIds = filesExcludingReplacements
                        .WhereAsync(
                            async file =>
                            {
                                // Not optimal, ideally we should be able to fetch
                                // the status with the file reference itself.
                                // TODO EES-1231 Move imports table into database
                                var importStatus = await _importStatusService
                                    .GetImportStatus(file.ReleaseId, file.Filename);

                                return importStatus.Status == IStatus.COMPLETE;
                            }
                        )
                        .Select(file => file.SubjectId)
                        .ToList();

                    var subjects = _statisticsDbContext.ReleaseSubject
                        .Include(subject => subject.Subject)
                        .Where(subject => subject.ReleaseId == releaseId
                                          && subjectIds.Contains(subject.SubjectId))
                        .Select(subject =>
                            new IdLabel(
                                subject.Subject.Id,
                                subject.Subject.Name))
                        .ToList();

                    var highlights = _contentDbContext.ReleaseContentBlocks
                        .Include(rcb => rcb.ContentBlock)
                        .Where(rcb => rcb.ReleaseId == release.Id)
                        .Select(rcb => rcb.ContentBlock)
                        .OfType<DataBlock>()
                        .Where(dataBlock => !string.IsNullOrEmpty(dataBlock.HighlightName))
                        .ToList()
                        // Need to query on materialized list due to JSON serialized query
                        .Where(dataBlock => subjectIds.Contains(dataBlock.Query.SubjectId))
                        .Select(dataBlock => new IdLabel(dataBlock.Id, dataBlock.HighlightName))
                        .ToList();

                    return new ReleaseSubjectsMetaViewModel
                    {
                        ReleaseId = releaseId,
                        Highlights = highlights,
                        Subjects = subjects,
                    };
                });
        }
    }
}