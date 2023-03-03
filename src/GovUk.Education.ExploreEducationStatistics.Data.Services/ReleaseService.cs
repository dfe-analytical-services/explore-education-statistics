#nullable enable
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
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IUserService _userService;
        private readonly IDataGuidanceSubjectService _dataGuidanceSubjectService;
        private readonly ITimePeriodService _timePeriodService;

        public ReleaseService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
            StatisticsDbContext statisticsDbContext,
            IUserService userService,
            IDataGuidanceSubjectService dataGuidanceSubjectService,
            ITimePeriodService timePeriodService)
        {
            _contentDbContext = contentDbContext;
            _contentPersistenceHelper = contentPersistenceHelper;
            _statisticsDbContext = statisticsDbContext;
            _userService = userService;
            _dataGuidanceSubjectService = dataGuidanceSubjectService;
            _timePeriodService = timePeriodService;
        }

        public async Task<Either<ActionResult, List<SubjectViewModel>>> ListSubjects(Guid releaseId)
        {
            return await _contentPersistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(_userService.CheckCanViewRelease)
                .OnSuccess(async _ =>
                {
                    var subjectsToInclude = GetPublishedSubjectIds(releaseId);

                    return await GetSubjects(releaseId, subjectsToInclude);
                });
        }

        private async Task<List<SubjectViewModel>> GetSubjects(Guid releaseId, List<Guid> subjectsToInclude)
        {
            if (subjectsToInclude.Count == 0)
            {
                return new List<SubjectViewModel>();
            }

            var releaseSubjects = await _statisticsDbContext
                .ReleaseSubject
                .AsQueryable()
                .Where(rs => rs.ReleaseId == releaseId && subjectsToInclude.Contains(rs.SubjectId))
                .ToListAsync();

            var releaseFiles = await QueryReleaseDataFiles(releaseId)
                .Where(rf => rf.File.SubjectId.HasValue
                             && subjectsToInclude.Contains(rf.File.SubjectId.Value))
                .ToListAsync();

            return (await releaseSubjects
                    .SelectAsync(
                        async rs =>
                        {
                            var releaseFile = releaseFiles.First(rf => rf.File.SubjectId == rs.SubjectId);

                            return new SubjectViewModel(
                                id: rs.SubjectId,
                                name: releaseFile.Name ?? string.Empty,
                                order: releaseFile.Order,
                                content: rs.DataGuidance ?? string.Empty,
                                timePeriods: await _timePeriodService.GetTimePeriodLabels(rs.SubjectId),
                                geographicLevels: await _dataGuidanceSubjectService.GetGeographicLevels(rs.SubjectId),
                                file: releaseFile.ToFileInfo()
                            );
                        }
                    ))
                .OrderBy(svm => svm.Order)
                .ThenBy(svm => svm.Name) // For subjects existing before ordering was added
                .ToList();
        }

        public async Task<Either<ActionResult, List<FeaturedTableViewModel>>> ListFeaturedTables(Guid releaseId)
        {
            var subjectsToInclude = GetPublishedSubjectIds(releaseId);

            var releaseContentBlocks = await _contentDbContext.ReleaseContentBlocks
                .Include(rcb => rcb.ContentBlock)
                .Where(rcb => rcb.ReleaseId == releaseId)
                .Select(rcb => rcb.ContentBlock)
                .OfType<DataBlock>()
                .Where(dataBlock => !string.IsNullOrEmpty(dataBlock.HighlightName))
                .ToListAsync();

            // Need to query on materialized list due to JSON serialized query
            return releaseContentBlocks
                .Where(dataBlock => subjectsToInclude.Contains(dataBlock.Query.SubjectId))
                .Select(
                    dataBlock => new FeaturedTableViewModel(
                        id: dataBlock.Id,
                        name: dataBlock.HighlightName ?? string.Empty,
                        description: dataBlock.HighlightDescription ?? string.Empty
                    )
                )
                .OrderBy(featuredTable => featuredTable.Name)
                .ToList();
        }

        private List<Guid> GetPublishedSubjectIds(Guid releaseId)
        {
            return QueryReleaseDataFiles(releaseId)
                .Join(
                    _contentDbContext.DataImports,
                    releaseFile => releaseFile.File,
                    import => import.File,
                    (releaseFile, import) => new
                    {
                        ReleaseFile = releaseFile,
                        DataImport = import
                    }
                )
                .Where(join => join.DataImport.Status == DataImportStatus.COMPLETE)
                .Select(join => join.ReleaseFile.File.SubjectId!.Value)
                .ToList();
        }

        private IQueryable<ReleaseFile> QueryReleaseDataFiles(Guid releaseId)
        {
            return _contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Where(
                    rf => rf.ReleaseId == releaseId
                          && rf.File.Type == FileType.Data
                          // Exclude files that are replacements in progress
                          && !rf.File.ReplacingId.HasValue
                          && rf.File.SubjectId.HasValue
                );
        }
    }
}
