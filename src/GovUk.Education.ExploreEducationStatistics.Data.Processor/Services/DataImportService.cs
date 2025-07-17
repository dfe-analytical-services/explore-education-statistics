#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.DataImportStatus;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class DataImportService : IDataImportService
    {
        private readonly IDbContextSupplier _dbContextSupplier;
        private readonly ILogger<DataImportService> _logger;

        public DataImportService(
            IDbContextSupplier dbContextSupplier,
            ILogger<DataImportService> logger)
        {
            _dbContextSupplier = dbContextSupplier;
            _logger = logger;
        }

        public async Task FailImport(Guid id, List<DataImportError> errors)
        {
            await using var contentDbContext = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            
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
            await FailImport(id, errors
                .Select(error => new DataImportError(error))
                .ToList());
        }

        public async Task<DataImport> GetImport(Guid id)
        {
            await using var contentDbContext = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            return await contentDbContext.DataImports
                .AsNoTracking()
                .Include(import => import.Errors)
                .Include(import => import.File)
                .Include(import => import.MetaFile)
                .Include(import => import.ZipFile)
                .SingleAsync(import => import.Id == id);
        }

        public async Task<DataImportStatus> GetImportStatus(Guid id)
        {
            await using var contentDbContext = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            var import = await contentDbContext.DataImports
                .AsNoTracking()
                .SingleOrDefaultAsync(i => i.Id == id);

            if (import == null)
            {
                return NOT_FOUND;
            }

            return import.Status;
        }

        public async Task Update(
            Guid id,
            int? expectedImportedRows = null,
            int? totalRows = null,
            HashSet<GeographicLevel>? geographicLevels = null,
            int? importedRows = null,
            int? lastProcessedRowIndex = null)
        {
            await using var contentDbContext = _dbContextSupplier.CreateDbContext<ContentDbContext>();
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
            await using var context = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            
            var import = await context.DataImports
                .Include(i => i.File)
                .SingleAsync(i => i.Id == id);

            var filename = import.File.Filename;

            var percentageCompleteBefore = import.StagePercentageComplete;
            var percentageCompleteAfter = (int) Math.Clamp(percentageComplete, 0, 100);

            // Ignore updating if already finished, or in the process of aborting and this status update isn't a
            // finishing status update. 
            if (import.Status.IsFinished() || (import.Status.IsAborting() && !newStatus.IsFinished()))
            {
                _logger.LogWarning(
                    "Update: {Filename} {ImportStatus} ({PercentageCompleteBefore}%) -> " +
                    "{NewStatus} ({PercentageCompleteAfter}%) ignored as this import is already in finished or " +
                    "completed state state {FinishedImportStatus}",
                    filename,
                    import.Status,
                    percentageCompleteBefore,
                    newStatus,
                    percentageCompleteAfter,
                    import.Status);
                
                return;
            }

            // Ignore updating to an equal percentage complete (after rounding) at the same status without logging it
            if (import.Status == newStatus && percentageCompleteBefore == percentageCompleteAfter)
            {
                return;
            }

            _logger.LogInformation(
                "Update: {Filename} {ImportStatus} ({PercentageCompleteBefore}%) -> {NewStatus} ({PercentageCompleteAfter}%)",
                filename,
                import.Status,
                percentageCompleteBefore,
                newStatus,
                percentageCompleteAfter);

            import.StagePercentageComplete = percentageCompleteAfter;
            import.Status = newStatus;
            context.DataImports.Update(import);
            await context.SaveChangesAsync();
        }

        public async Task WriteDataSetFileMeta(Guid fileId, Guid subjectId, int? numDataFileRows)
        {
            await using var contentDbContext = _dbContextSupplier.CreateDbContext<ContentDbContext>();
            await using var statisticsDbContext = _dbContextSupplier.CreateDbContext<StatisticsDbContext>();

            var observations = statisticsDbContext.Observation
                .AsNoTracking()
                .Where(o => o.SubjectId == subjectId);

            var geographicLevels = observations
                .Select(o => o.Location.GeographicLevel)
                .Distinct()
                .OrderBy(gl => gl)
                .ToList();

            var timePeriods = observations
                .Select(o => new { o.Year, o.TimeIdentifier, })
                .Distinct()
                .OrderBy(o => o.Year)
                .ThenBy(o => o.TimeIdentifier)
                .ToList()
                .Select(tp => new TimePeriodRangeBoundMeta
                {
                    Period = tp.Year.ToString(),
                    TimeIdentifier = tp.TimeIdentifier,
                })
                .ToList();

            var filters = await statisticsDbContext.Filter
                .AsNoTracking()
                .Where(f => f.SubjectId == subjectId)
                .OrderBy(f => f.Label)
                .Select(f => new FilterMeta
                {
                    Id = f.Id,
                    Label = f.Label,
                    Hint = f.Hint,
                    ColumnName = f.Name,
                    ParentFilter = f.ParentFilter,
                })
                .ToListAsync();

            var indicators = await statisticsDbContext.Indicator
                .AsNoTracking()
                .Where(i => i.IndicatorGroup.SubjectId == subjectId)
                .Select(i => new IndicatorMeta
                {
                    Id = i.Id,
                    Label = i.Label,
                    ColumnName = i.Name,
                })
                .OrderBy(i => i.Label)
                .ToListAsync();

            var dataSetFileMeta = new DataSetFileMeta
            {
                NumDataFileRows = numDataFileRows,
                TimePeriodRange = new TimePeriodRangeMeta
                {
                    Start = timePeriods.First(),
                    End = timePeriods.Last(),
                },
                Filters = filters,
                Indicators = indicators,
            };

            var file = contentDbContext.Files
                .Single(f => f.Type == FileType.Data
                             && f.SubjectId == subjectId);
            file.DataSetFileMeta = dataSetFileMeta;

            var dataSetFileVersionGeographicLevels = geographicLevels
                .Select(gl => new DataSetFileVersionGeographicLevel
                {
                    DataSetFileVersionId = fileId,
                    GeographicLevel = gl,
                }).ToList();
            contentDbContext.DataSetFileVersionGeographicLevels.AddRange(
                dataSetFileVersionGeographicLevels);

            file.FilterHierarchies = await GenerateFilterHierarchies(statisticsDbContext, filters);

            await contentDbContext.SaveChangesAsync();
        }

        public static async Task<List<DataSetFileFilterHierarchy>> GenerateFilterHierarchies(
            StatisticsDbContext statisticsDbContext,
            List<FilterMeta> filters)
        {
            var rootFilters = filters
                .Where(parentFilter => parentFilter.ParentFilter == null
                                       && filters.Any(childFilter =>
                                           parentFilter.ColumnName == childFilter.ParentFilter));

            var hierarchies = new List<DataSetFileFilterHierarchy>();

            foreach (var rootFilter in rootFilters)
            {
                var hierarchy = await GenerateFilterHierarchy(statisticsDbContext, rootFilter, filters);
                hierarchies.Add(hierarchy);
            }

            return hierarchies;
        }

        private static async Task<DataSetFileFilterHierarchy> GenerateFilterHierarchy(
            StatisticsDbContext statisticsDbContext,
            FilterMeta rootFilter,
            List<FilterMeta> filters)
        {
            var rootFilterItemIds = statisticsDbContext.FilterItem
                .AsNoTracking()
                .Where(fi => fi.FilterGroup.FilterId == rootFilter.Id)
                .Select(fi => fi.Id)
                .ToHashSet();

            var filterIds = new List<Guid>();
            var tiers = new List<Dictionary<Guid, List<Guid>>>();

            var parentFilter = rootFilter;
            var parentFilterItemIds = rootFilterItemIds;
            var childFilter = filters
                .Single(f => f.ParentFilter == parentFilter.ColumnName);

            filterIds.Add(rootFilter.Id);

            // Loop over each parent/child or tier, starting with the root filter, until no child is found
            while (true)
            {
                var currentParentFilterId = parentFilter.Id; // avoid closure madness
                var currentChildFilterId = childFilter.Id;

                filterIds.Add(currentChildFilterId);

                var filterItemRelationships = await statisticsDbContext.FilterItem
                    .AsNoTracking()
                    .Where(fi => fi.FilterGroup.FilterId == currentParentFilterId)
                    .SelectMany(parentFilterItem =>
                        statisticsDbContext.ObservationFilterItem
                            .AsNoTracking()
                            .Where(childOfi =>
                                childOfi.FilterId == currentChildFilterId
                                && statisticsDbContext.ObservationFilterItem.Any(parentOfi =>
                                    childOfi.ObservationId == parentOfi.ObservationId
                                    && parentOfi.FilterItemId == parentFilterItem.Id))
                            .Select(childOfi => new
                            {
                                FilterItemId = childOfi.FilterItem.Id,
                                ParentItemId = parentFilterItem.Id,
                            })
                            .ToList())
                    .Distinct()
                    .ToListAsync();

                var tier = new Dictionary<Guid, List<Guid>>();
                foreach (var parentFilterItemId in parentFilterItemIds)
                {
                    var childFilterItemIdsForParentItem = filterItemRelationships
                        .Where(childFilterItem => childFilterItem.ParentItemId == parentFilterItemId)
                        .Select(childFilterItem => childFilterItem.FilterItemId)
                        .ToList();

                    tier.Add(parentFilterItemId, childFilterItemIdsForParentItem);
                }

                tiers.Add(tier);

                // check whether we're finished
                var newChildFilter = filters
                    .SingleOrDefault(newChildFilter => newChildFilter.ParentFilter == childFilter.ColumnName);
                if (newChildFilter == null)
                {
                    break;
                }

                // if not finished, prepare for next iteration of loop
                parentFilter = childFilter;
                childFilter = newChildFilter;
                parentFilterItemIds = filterItemRelationships
                    .Select(childFilterItem => childFilterItem.FilterItemId)
                    .ToHashSet();
            }

            return new DataSetFileFilterHierarchy(
                FilterIds: filterIds,
                Tiers: tiers);
        }
    }
}
