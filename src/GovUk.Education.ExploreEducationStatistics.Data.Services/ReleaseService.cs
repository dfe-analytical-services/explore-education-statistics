#nullable enable
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
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
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
    private readonly ITimePeriodService _timePeriodService;

    public ReleaseService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
        StatisticsDbContext statisticsDbContext,
        IUserService userService,
        ITimePeriodService timePeriodService
    )
    {
        _contentDbContext = contentDbContext;
        _contentPersistenceHelper = contentPersistenceHelper;
        _statisticsDbContext = statisticsDbContext;
        _userService = userService;
        _timePeriodService = timePeriodService;
    }

    public async Task<Either<ActionResult, List<SubjectViewModel>>> ListSubjects(Guid releaseVersionId)
    {
        return await _contentPersistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId, query => query.Include(rv => rv.Release))
            .OnSuccess(_userService.CheckCanViewReleaseVersion)
            .OnSuccess(async _ =>
            {
                var subjectsToInclude = GetPublishedSubjectIds(releaseVersionId);

                return await GetSubjects(releaseVersionId, subjectsToInclude);
            });
    }

    private async Task<List<SubjectViewModel>> GetSubjects(Guid releaseVersionId, List<Guid> subjectsToInclude)
    {
        if (subjectsToInclude.Count == 0)
        {
            return new List<SubjectViewModel>();
        }

        var releaseFiles = await QueryReleaseDataFiles(releaseVersionId)
            .Where(rf => rf.File.SubjectId.HasValue && subjectsToInclude.Contains(rf.File.SubjectId.Value))
            .ToListAsync();

        return (
            await releaseFiles.SelectAsync(async releaseFile =>
            {
                var subjectId = releaseFile.File.SubjectId!.Value;

                return new SubjectViewModel(
                    id: subjectId,
                    name: releaseFile.Name ?? string.Empty,
                    order: releaseFile.Order,
                    content: releaseFile.Summary ?? string.Empty,
                    timePeriods: await _timePeriodService.GetTimePeriodLabels(subjectId),
                    geographicLevels: await GetGeographicLevels(subjectId),
                    geographicLevelsCsvOnly: await GetGeographicLevels(subjectId, csvOnly: true),
                    filters: await GetFilters(subjectId, releaseFile.FilterSequence),
                    indicators: await GetIndicators(subjectId, releaseFile.IndicatorSequence),
                    file: releaseFile.ToFileInfo(),
                    lastUpdated: releaseFile.Published
                );
            })
        ).OrderBy(svm => svm.Order).ThenBy(svm => svm.Name) // For subjects existing before ordering was added
        .ToList();
    }

    private async Task<List<string>> GetGeographicLevels(Guid subjectId, bool csvOnly = false)
    {
        var geographicLevels = await _contentDbContext
            .Files.AsNoTracking()
            .Where(file => file.SubjectId == subjectId && file.Type == FileType.Data)
            .SelectMany(file => file.DataSetFileVersionGeographicLevels)
            .Where(gl => gl.CsvOnly == csvOnly)
            .Select(gl => gl.GeographicLevel)
            .ToListAsync();

        return geographicLevels.Select(gl => gl.GetEnumLabel()).Order().ToList();
    }

    private async Task<List<string>> GetFilters(Guid subjectId, List<FilterSequenceEntry>? filterSequence)
    {
        var filters = await _statisticsDbContext
            .Filter.AsNoTracking()
            .Where(filter => filter.SubjectId == subjectId)
            .ToListAsync();

        return MetaViewModelBuilderUtils
            .OrderBySequenceOrLabel(
                filters,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: sequenceEntry => sequenceEntry.Id,
                resultSelector: value => value.Value.Label,
                filterSequence
            )
            .ToList();
    }

    private async Task<List<string>> GetIndicators(
        Guid subjectId,
        List<IndicatorGroupSequenceEntry>? indicatorGroupSequence
    )
    {
        var indicators = await _statisticsDbContext
            .Indicator.AsNoTracking()
            .Where(indicator => indicator.IndicatorGroup.SubjectId == subjectId)
            .ToListAsync();

        var indicatorSequence = indicatorGroupSequence?.SelectMany(seq => seq.ChildSequence);
        return MetaViewModelBuilderUtils
            .OrderBySequenceOrLabel(
                indicators,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: sequenceEntry => sequenceEntry,
                resultSelector: value => value.Value.Label,
                indicatorSequence
            )
            .ToList();
    }

    public async Task<Either<ActionResult, List<FeaturedTableViewModel>>> ListFeaturedTables(Guid releaseVersionId)
    {
        var publishedSubjectIds = GetPublishedSubjectIds(releaseVersionId);

        var releaseDataBlockList = (
            await _contentDbContext
                .DataBlockVersions.Where(dataBlockVersion => dataBlockVersion.ReleaseVersionId == releaseVersionId)
                .Select(db => new { db.Id, db.Query })
                .ToListAsync()
        ) // we need to materialise the list access `dataBlockVersion.Query.SubjectId` as `Query` is json
            .Where(dataBlockVersion => publishedSubjectIds.Contains(dataBlockVersion.Query.SubjectId))
            .ToList();

        var releaseDataBlockVersionIdList = releaseDataBlockList.Select(db => db.Id).ToList();

        var featuredTables = await _contentDbContext
            .FeaturedTables.Include(ft => ft.DataBlockVersion)
            .Where(ft => releaseDataBlockVersionIdList.Contains(ft.DataBlockVersionId))
            .OrderBy(ft => ft.Order)
            .ThenBy(ft => ft.Name)
            .ToListAsync();

        return featuredTables
            .Select(ft => new FeaturedTableViewModel(
                ft.Id,
                ft.Name,
                ft.Description,
                ft.DataBlockVersion.Query.SubjectId,
                ft.DataBlockVersionId,
                ft.DataBlockId,
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
