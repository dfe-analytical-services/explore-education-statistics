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
                                filters: await GetFilters(rs.SubjectId, rs.FilterSequence),
                                indicators: await GetIndicators(rs.SubjectId, rs.IndicatorSequence),
                                file: releaseFile.ToFileInfo()
                            );
                        }
                    ))
                .OrderBy(svm => svm.Order)
                .ThenBy(svm => svm.Name) // For subjects existing before ordering was added
                .ToList();
        }

        private async Task<List<string>> GetFilters(Guid subjectId, List<FilterSequenceEntry>? filterSequence)
        {
            var unorderedFilterList = await _statisticsDbContext.Filter
                .Where(filter => filter.SubjectId == subjectId)
                .ToListAsync();

            if (filterSequence == null)
            {
                return unorderedFilterList
                    .Select(filter => filter.Label)
                    .OrderBy(label => label)
                    .ToList();
            }

            var filterIdSequence = filterSequence
                .Select(filter => filter.Id)
                .ToList();

            return unorderedFilterList
                .OrderBy(filter => filterIdSequence.IndexOf(filter.Id))
                .Select(filter => filter.Label)
                .ToList();
        }

        private async Task<List<string>> GetIndicators(
            Guid subjectId,
            List<IndicatorGroupSequenceEntry>? indicatorGroupSequence)
        {
            var unorderedIndicators= await _statisticsDbContext.Indicator
                .Where(indicator => indicator.IndicatorGroup.SubjectId == subjectId)
                .ToListAsync();

            if (indicatorGroupSequence == null)
            {
                return unorderedIndicators
                    .Select(indicator => indicator.Label)
                    .OrderBy(label => label)
                    .ToList();
            }

            var indicatorIdSequence = indicatorGroupSequence
                .SelectMany(indicatorGroup => indicatorGroup.ChildSequence)
                .ToList();

            return unorderedIndicators
                .OrderBy(indicator => indicatorIdSequence.IndexOf(indicator.Id))
                .Select(indicator => indicator.Label)
                .ToList();
        }


        public async Task<Either<ActionResult, List<FeaturedTableViewModel>>> ListFeaturedTables(Guid releaseId)
        {
            var publishedSubjectIds = GetPublishedSubjectIds(releaseId);

            var releaseDataBlockList = (await _contentDbContext.ReleaseContentBlocks
                    .Include(rcb => rcb.ContentBlock)
                    .Where(rcb => rcb.ReleaseId == releaseId)
                    .Select(rcb => rcb.ContentBlock)
                    .OfType<DataBlock>()
                    .ToListAsync()) // we need to materialise the list access `dataBlock.Query.SubjectId` as `Query` is json
                .Where(dataBlock => publishedSubjectIds.Contains(dataBlock.Query.SubjectId))
                .ToList();

            var releaseDataBlockIdList = releaseDataBlockList.Select(db => db.Id).ToList();

            var featuredTables = await _contentDbContext.FeaturedTables
                .Include(ft => ft.DataBlock)
                .Where(ft => releaseDataBlockIdList.Contains(ft.DataBlockId))
                .OrderBy(ft => ft.Order)
                .ThenBy(ft => ft.Name)
                .ToListAsync();

            return featuredTables
                .Select(ft => new FeaturedTableViewModel(
                    ft.Id, ft.Name, ft.Description, ft.DataBlock.Query.SubjectId, ft.DataBlockId, ft.Order))
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
