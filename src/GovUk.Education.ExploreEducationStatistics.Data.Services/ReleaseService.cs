#nullable enable
using System.Data;
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
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseVersion;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;

public class ReleaseService : IReleaseService
{
    private readonly ContentDbContext _contentDbContext;
    private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;
    private readonly StatisticsDbContext _statisticsDbContext;
    private readonly IUserService _userService;
    private readonly IDataGuidanceDataSetService _dataGuidanceDataSetService;
    private readonly ITimePeriodService _timePeriodService;

    public ReleaseService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        StatisticsDbContext statisticsDbContext,
        IUserService userService,
        IDataGuidanceDataSetService dataGuidanceDataSetService,
        ITimePeriodService timePeriodService
    )
    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _statisticsDbContext = statisticsDbContext;
        _userService = userService;
        _dataGuidanceDataSetService = dataGuidanceDataSetService;
        _timePeriodService = timePeriodService;
    }

    public async Task<Either<ActionResult, List<SubjectViewModel>>> ListSubjects(
        Guid releaseVersionId
    )
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async _ =>
            {
                var subjectsToInclude = GetPublishedSubjectIds(releaseVersionId);

                return await GetSubjects(releaseVersionId, subjectsToInclude);
            });
    }

    private async Task<List<SubjectViewModel>> GetSubjects(
        Guid releaseVersionId,
        List<Guid> subjectsToInclude
    )
    {
        if (subjectsToInclude.Count == 0)
        {
            return new List<SubjectViewModel>();
        }

        var releaseSubjects = await _statisticsDbContext
            .ReleaseSubject.AsQueryable()
            .Where(rs =>
                rs.ReleaseVersionId == releaseVersionId && subjectsToInclude.Contains(rs.SubjectId)
            )
            .ToListAsync();

        if (
            !ComparerUtils.SequencesAreEqualIgnoringOrder(
                releaseSubjects.Select(rs => rs.SubjectId).ToList(),
                subjectsToInclude
            )
        )
        {
            throw new DataException(
                $"""
                Statistics DB has a different subjects than the Content DB
                StatsDB subjects: {releaseSubjects.Select(rs => rs.SubjectId).JoinToString(',')}
                ContentDb subjects: {subjectsToInclude.JoinToString(',')}
                """
            );
        }

        var releaseFiles = await QueryReleaseDataFiles(releaseVersionId)
            .Where(rf =>
                rf.File.SubjectId.HasValue && subjectsToInclude.Contains(rf.File.SubjectId.Value)
            )
            .ToListAsync();

        return (
            await releaseSubjects.SelectAsync(async rs =>
            {
                var releaseFile = releaseFiles.First(rf => rf.File.SubjectId == rs.SubjectId);

                return new SubjectViewModel(
                    id: rs.SubjectId,
                    name: releaseFile.Name ?? string.Empty,
                    order: releaseFile.Order,
                    content: releaseFile.Summary ?? string.Empty,
                    timePeriods: await _timePeriodService.GetTimePeriodLabels(rs.SubjectId),
                    geographicLevels: await _dataGuidanceDataSetService.ListGeographicLevels(
                        rs.SubjectId
                    ),
                    filters: await GetFilters(rs.SubjectId, releaseFile.FilterSequence),
                    indicators: await GetIndicators(rs.SubjectId, releaseFile.IndicatorSequence),
                    file: releaseFile.ToFileInfo(),
                    lastUpdated: releaseFile.Published
                );
            })
        ).OrderBy(svm => svm.Order).ThenBy(svm => svm.Name) // For subjects existing before ordering was added
        .ToList();
    }

    private async Task<List<string>> GetFilters(
        Guid subjectId,
        List<FilterSequenceEntry>? filterSequence
    )
    {
        var unorderedFilterList = await _statisticsDbContext
            .Filter.Where(filter => filter.SubjectId == subjectId)
            .ToListAsync();

        if (filterSequence == null)
        {
            return unorderedFilterList
                .Select(filter => filter.Label)
                .OrderBy(label => label)
                .ToList();
        }

        var filterIdSequence = filterSequence.Select(filter => filter.Id).ToList();

        return unorderedFilterList
            .OrderBy(filter => filterIdSequence.IndexOf(filter.Id))
            .Select(filter => filter.Label)
            .ToList();
    }

    private async Task<List<string>> GetIndicators(
        Guid subjectId,
        List<IndicatorGroupSequenceEntry>? indicatorGroupSequence
    )
    {
        var unorderedIndicators = await _statisticsDbContext
            .Indicator.Where(indicator => indicator.IndicatorGroup.SubjectId == subjectId)
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

    public async Task<Either<ActionResult, List<FeaturedTableViewModel>>> ListFeaturedTables(
        Guid releaseVersionId
    )
    {
        var publishedSubjectIds = GetPublishedSubjectIds(releaseVersionId);

        var releaseDataBlockList = (
            await _contentDbContext
                .ContentBlocks.Where(block => block.ReleaseVersionId == releaseVersionId)
                .OfType<DataBlock>()
                .Select(db => new { db.Id, db.Query })
                .ToListAsync()
        ) // we need to materialise the list access `dataBlock.Query.SubjectId` as `Query` is json
            .Where(dataBlock => publishedSubjectIds.Contains(dataBlock.Query.SubjectId))
            .ToList();

        var releaseDataBlockIdList = releaseDataBlockList.Select(db => db.Id).ToList();

        var featuredTables = await _contentDbContext
            .FeaturedTables.Include(ft => ft.DataBlock)
            .Where(ft => releaseDataBlockIdList.Contains(ft.DataBlockId))
            .OrderBy(ft => ft.Order)
            .ThenBy(ft => ft.Name)
            .ToListAsync();

        return featuredTables
            .Select(ft => new FeaturedTableViewModel(
                ft.Id,
                ft.Name,
                ft.Description,
                ft.DataBlock.Query.SubjectId,
                ft.DataBlockId,
                ft.DataBlockParentId,
                ft.Order
            ))
            .ToList();
    }

    private List<Guid> GetPublishedSubjectIds(Guid releaseVersionId)
    {
        return QueryReleaseDataFiles(releaseVersionId)
            .Join(
                _contentDbContext.DataImports,
                releaseFile => releaseFile.File,
                import => import.File,
                (releaseFile, import) => new { ReleaseFile = releaseFile, DataImport = import }
            )
            .Where(join => join.DataImport.Status == DataImportStatus.COMPLETE)
            .Select(join => join.ReleaseFile.File.SubjectId!.Value)
            .ToList();
    }

    private IQueryable<ReleaseFile> QueryReleaseDataFiles(Guid releaseVersionId)
    {
        return _contentDbContext
            .ReleaseFiles.Include(rf => rf.File)
            .Where(rf =>
                rf.ReleaseVersionId == releaseVersionId
                && rf.File.Type == FileType.Data
                // Exclude files that are replacements in progress
                && !rf.File.ReplacingId.HasValue
                && rf.File.SubjectId.HasValue
            );
    }
}
