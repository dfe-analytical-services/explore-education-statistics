using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IDataImportRepository _dataImportRepository;
        private readonly IUserService _userService;
        private readonly IMetaGuidanceSubjectService _metaGuidanceSubjectService;

        public ReleaseService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            StatisticsDbContext statisticsDbContext,
            IDataImportRepository dataImportRepository,
            IUserService userService,
            IMetaGuidanceSubjectService metaGuidanceSubjectService)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _statisticsDbContext = statisticsDbContext;
            _dataImportRepository = dataImportRepository;
            _userService = userService;
            _metaGuidanceSubjectService = metaGuidanceSubjectService;
        }

        public async Task<Either<ActionResult, ReleaseViewModel>> GetRelease(Guid releaseId)
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
                        var importStatus = await _dataImportRepository.GetStatusByFileId(file.Id);
                        if (importStatus == DataImportStatus.COMPLETE && file.SubjectId.HasValue)
                        {
                            subjectsToInclude.Add(file.SubjectId.Value);
                        }
                    }

                    var subjects = await GetSubjects(releaseId, subjectsToInclude);

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

                    return new ReleaseViewModel
                    {
                        Id = releaseId,
                        Highlights = highlights,
                        Subjects = subjects,
                    };
                });
        }

        private async Task<List<SubjectViewModel>> GetSubjects(Guid releaseId, List<Guid> subjectsToInclude)
        {
            var releaseSubjects = await _statisticsDbContext.ReleaseSubject
                .Include(subject => subject.Subject)
                .Where(
                    rs => rs.ReleaseId == releaseId
                          && subjectsToInclude.Contains(rs.SubjectId)
                )
                .OrderBy(rs => rs.Subject.Name)
                .ToListAsync();

            return (await releaseSubjects
                .SelectAsync(
                    async rs =>
                        new SubjectViewModel(
                            id: rs.Subject.Id,
                            name: rs.Subject.Name,
                            content: rs.MetaGuidance,
                            timePeriods: await _metaGuidanceSubjectService.GetTimePeriods(rs.SubjectId),
                            geographicLevels: await _metaGuidanceSubjectService.GetGeographicLevels(rs.SubjectId)
                        )
                ))
                .ToList();
        }
    }
}