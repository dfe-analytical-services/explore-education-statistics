#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services;

public class DataImportService(
    IDbContextSupplier dbContextSupplier,
    IStorageDataSetResolver storageDataSetResolver,
    ILogger<DataImportService> logger
) : IDataImportService
{
    public async Task FailImport(Guid id, List<DataImportError> errors)
    {
        await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();

        var import = await contentDbContext.DataImports.SingleAsync(d => d.Id == id);

        if (import.Status != COMPLETE && import.Status != FAILED)
        {
            contentDbContext.Update(import);
            import.Status = FAILED;
            import.Errors.AddRange(errors);

            await contentDbContext.SaveChangesAsync();
        }
    }

    public async Task FailImport(Guid id, params string[] errors)
    {
        await FailImport(id, errors.Select(error => new DataImportError(error)).ToList());
    }

    public async Task<DataImport> GetImport(Guid id)
    {
        await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();
        return await contentDbContext
            .DataImports.AsNoTracking()
            .Include(import => import.Errors)
            .Include(import => import.File)
            .Include(import => import.MetaFile)
            .SingleAsync(import => import.Id == id);
    }

    public async Task<DataImportStatus> GetImportStatus(Guid id)
    {
        await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();
        var import = await contentDbContext.DataImports.AsNoTracking().SingleOrDefaultAsync(i => i.Id == id);

        return import?.Status ?? NOT_FOUND;
    }

    public async Task Update(
        Guid id,
        int? expectedImportedRows = null,
        int? totalRows = null,
        HashSet<GeographicLevel>? geographicLevels = null,
        int? importedRows = null,
        int? lastProcessedRowIndex = null
    )
    {
        await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();
        var import = await contentDbContext.DataImports.SingleAsync(import => import.Id == id);
        contentDbContext.Update(import);

        import.ExpectedImportedRows = expectedImportedRows ?? import.ExpectedImportedRows;
        import.TotalRows = totalRows ?? import.TotalRows;
        import.GeographicLevels = geographicLevels ?? import.GeographicLevels;
        import.ImportedRows = importedRows ?? import.ImportedRows;
        import.LastProcessedRowIndex = lastProcessedRowIndex ?? import.LastProcessedRowIndex;

        await contentDbContext.SaveChangesAsync();
    }

    public async Task UpdateStatus(Guid id, DataImportStatus newStatus, double percentageComplete)
    {
        await using var context = dbContextSupplier.CreateDbContext<ContentDbContext>();

        var import = await context.DataImports.Include(i => i.File).SingleAsync(i => i.Id == id);

        var filename = import.File.Filename;

        var percentageCompleteBefore = import.StagePercentageComplete;
        var percentageCompleteAfter = (int)Math.Clamp(percentageComplete, 0, 100);

        // Ignore updating if already finished, or in the process of aborting and this status update isn't a
        // finishing status update.
        if (import.Status.IsFinished() || (import.Status.IsAborting() && !newStatus.IsFinished()))
        {
            logger.LogWarning(
                "Update: {Filename} {ImportStatus} ({PercentageCompleteBefore}%) -> "
                    + "{NewStatus} ({PercentageCompleteAfter}%) ignored as this import is already in finished or "
                    + "completed state state {FinishedImportStatus}",
                filename,
                import.Status,
                percentageCompleteBefore,
                newStatus,
                percentageCompleteAfter,
                import.Status
            );

            return;
        }

        // Ignore updating to an equal percentage complete (after rounding) at the same status without logging it
        if (import.Status == newStatus && percentageCompleteBefore == percentageCompleteAfter)
        {
            return;
        }

        logger.LogInformation(
            "Update: {Filename} {ImportStatus} ({PercentageCompleteBefore}%) -> {NewStatus} ({PercentageCompleteAfter}%)",
            filename,
            import.Status,
            percentageCompleteBefore,
            newStatus,
            percentageCompleteAfter
        );

        import.StagePercentageComplete = percentageCompleteAfter;
        import.Status = newStatus;
        context.DataImports.Update(import);
        await context.SaveChangesAsync();
    }

    public async Task WriteDataSetFileMeta(DataImport import)
    {
        var subjectId = import.SubjectId;

        await using var contentDbContext = dbContextSupplier.CreateDbContext<ContentDbContext>();

        var file = contentDbContext.Files.Single(f => f.Type == FileType.Data && f.SubjectId == subjectId);
        var dataSet = storageDataSetResolver.Resolve(file);

        var importedGeographicLevels = await dataSet.ListGeographicLevels();

        var timePeriods = (await dataSet.ListTimePeriods())
            .Select(tp => new TimePeriodRangeBoundMeta
            {
                Period = tp.Year.ToString(),
                TimeIdentifier = tp.TimeIdentifier,
            })
            .ToList();

        var filters = await dataSet.ListFilters();

        var filterMetas = filters
            .OrderBy(f => f.Label)
            .Select(f => new FilterMeta
            {
                Id = f.Id,
                Label = f.Label,
                Hint = f.Hint,
                ColumnName = f.Name,
                ParentFilter = f.ParentFilter,
            })
            .ToList();

        var indicators = (await dataSet.ListIndicators())
            .Select(i => new IndicatorMeta
            {
                Id = i.Id,
                Label = i.Label,
                ColumnName = i.Name,
            })
            .OrderBy(i => i.Label)
            .ToList();

        var dataSetFileMeta = new DataSetFileMeta
        {
            NumDataFileRows = import.TotalRows!.Value,
            TimePeriodRange = new TimePeriodRangeMeta { Start = timePeriods.First(), End = timePeriods.Last() },
            Filters = filterMetas,
            Indicators = indicators,
        };

        file.DataSetFileMeta = dataSetFileMeta;

        var csvGeographicLevels = import.GeographicLevels!;
        var dataSetFileVersionGeographicLevels = csvGeographicLevels
            .OrderBy(gl => gl)
            .Select(gl => new DataSetFileVersionGeographicLevel
            {
                DataSetFileVersionId = import.FileId,
                GeographicLevel = gl,
                CsvOnly = !importedGeographicLevels.Contains(gl),
            })
            .ToList();
        contentDbContext.DataSetFileVersionGeographicLevels.AddRange(dataSetFileVersionGeographicLevels);

        file.FilterHierarchies = await GenerateFilterHierarchies(dataSet, filters);

        await contentDbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Builds a hierarchy for each root filter (a filter with no parent that is the parent of another filter).
    /// The filters must include their FilterGroups and FilterItems, as returned by
    /// <see cref="IStorageDataSet.ListFilters" />.
    /// </summary>
    public static async Task<List<DataSetFileFilterHierarchy>> GenerateFilterHierarchies(
        IStorageDataSet dataSet,
        List<Filter> filters
    )
    {
        var rootFilters = filters.Where(parentFilter =>
            parentFilter.ParentFilter == null
            && filters.Any(childFilter => parentFilter.Name == childFilter.ParentFilter)
        );

        var hierarchies = new List<DataSetFileFilterHierarchy>();

        foreach (var rootFilter in rootFilters)
        {
            var hierarchy = await GenerateFilterHierarchy(dataSet, rootFilter, filters);
            hierarchies.Add(hierarchy);
        }

        return hierarchies;
    }

    private static async Task<DataSetFileFilterHierarchy> GenerateFilterHierarchy(
        IStorageDataSet dataSet,
        Filter rootFilter,
        List<Filter> filters
    )
    {
        var rootFilterItemIds = rootFilter
            .FilterGroups.SelectMany(fg => fg.FilterItems)
            .Select(fi => fi.Id)
            .ToHashSet();

        var filterIds = new List<Guid>();
        var tiers = new List<Dictionary<Guid, List<Guid>>>();

        var parentFilter = rootFilter;
        var parentFilterItemIds = rootFilterItemIds;
        var childFilter = filters.Single(f => f.ParentFilter == parentFilter.Name);

        filterIds.Add(rootFilter.Id);

        // Loop over each parent/child or tier, starting with the root filter, until no child is found
        while (true)
        {
            filterIds.Add(childFilter.Id);

            var filterItemRelationships = await dataSet.ListFilterItemRelationships(
                parentFilterId: parentFilter.Id,
                childFilterId: childFilter.Id
            );

            var tier = new Dictionary<Guid, List<Guid>>();
            foreach (var parentFilterItemId in parentFilterItemIds)
            {
                var childFilterItemIdsForParentItem = filterItemRelationships
                    .Where(relationship => relationship.ParentFilterItemId == parentFilterItemId)
                    .Select(relationship => relationship.ChildFilterItemId)
                    .ToList();

                tier.Add(parentFilterItemId, childFilterItemIdsForParentItem);
            }

            tiers.Add(tier);

            // check whether we're finished
            var newChildFilter = filters.SingleOrDefault(newChildFilter =>
                newChildFilter.ParentFilter == childFilter.Name
            );
            if (newChildFilter == null)
            {
                break;
            }

            // if not finished, prepare for next iteration of loop
            parentFilter = childFilter;
            childFilter = newChildFilter;
            parentFilterItemIds = filterItemRelationships
                .Select(relationship => relationship.ChildFilterItemId)
                .ToHashSet();
        }

        return new DataSetFileFilterHierarchy(FilterIds: filterIds, Tiers: tiers);
    }
}
