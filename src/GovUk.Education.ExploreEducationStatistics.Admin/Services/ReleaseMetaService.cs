using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
        private readonly IImportRepository _importRepository;
        private readonly IUserService _userService;

        public ReleaseMetaService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            StatisticsDbContext statisticsDbContext,
            IImportRepository importRepository,
            IUserService userService)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _statisticsDbContext = statisticsDbContext;
            _importRepository = importRepository;
            _userService = userService;
        }

        public async Task<Either<ActionResult, ReleaseSubjectsMetaViewModel>> GetSubjectsMeta(Guid releaseId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async release =>
                {
                    var files = _contentDbContext.ReleaseFiles
                        .Include(rf => rf.File)
                        .Where(rf => rf.ReleaseId == releaseId
                                       && rf.File.Type == FileType.Data
                                       // Exclude files that are replacements in progress
                                       && !rf.File.ReplacingId.HasValue)
                        .Select(file => file.File)
                        .ToList();

                    var subjectsToInclude = new List<Guid>();
                    foreach (var file in files)
                    {
                        var importStatus = await _importRepository.GetStatusByFileId(file.Id);
                        if (importStatus == ImportStatus.COMPLETE && file.SubjectId.HasValue)
                        {
                            subjectsToInclude.Add(file.SubjectId.Value);
                        }
                    }

                    var subjects = _statisticsDbContext.ReleaseSubject
                        .Include(subject => subject.Subject)
                        .Where(subject => subject.ReleaseId == releaseId
                                          && subjectsToInclude.Contains(subject.SubjectId))
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
                        .Where(dataBlock => subjectsToInclude.Contains(dataBlock.Query.SubjectId))
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