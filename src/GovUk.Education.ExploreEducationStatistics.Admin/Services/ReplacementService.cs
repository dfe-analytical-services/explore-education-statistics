using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Services.LocationService;
using IFootnoteService = GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces.IFootnoteService;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class ReplacementService : IReplacementService
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly StatisticsDbContext _statisticsDbContext;
        private readonly IFilterService _filterService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly IFootnoteService _footnoteService;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IPersistenceHelper<StatisticsDbContext> _persistenceHelper;

        public ReplacementService(ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IFilterService filterService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IFootnoteService footnoteService,
            ITimePeriodService timePeriodService,
            IPersistenceHelper<StatisticsDbContext> persistenceHelper)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _filterService = filterService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _footnoteService = footnoteService;
            _timePeriodService = timePeriodService;
            _persistenceHelper = persistenceHelper;
        }

        public async Task<Either<ActionResult, ReplacementPlanViewModel>> GetReplacementPlan(Guid originalSubjectId,
            Guid replacementSubjectId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Subject>(originalSubjectId)
                .OnSuccessDo(() => _persistenceHelper.CheckEntityExists<Subject>(replacementSubjectId))
                .OnSuccess(() => GetReleaseId(originalSubjectId, replacementSubjectId))
                .OnSuccess(releaseId =>
                {
                    var replacementSubjectMeta = GetReplacementSubjectMeta(replacementSubjectId);

                    var dataBlocks = ValidateDataBlocks(releaseId, originalSubjectId, replacementSubjectMeta);
                    var footnotes = ValidateFootnotes(releaseId, originalSubjectId, replacementSubjectMeta);

                    return new ReplacementPlanViewModel(dataBlocks, footnotes);
                });
        }

        public async Task<Either<ActionResult, Unit>> Replace(Guid originalSubjectId, Guid replacementSubjectId)
        {
            return await GetReplacementPlan(originalSubjectId, replacementSubjectId)
                .OnSuccess(async replacementPlan =>
                {
                    if (!replacementPlan.Valid)
                    {
                        return ValidationActionResult(ReplacementMustBeValid);
                    }

                    await replacementPlan.DataBlocks.ForEachAsync(plan => ReplaceLinksForDataBlock(plan, replacementSubjectId));
                    await replacementPlan.Footnotes.ForEachAsync(ReplaceLinksForFootnote);

                    await _contentDbContext.SaveChangesAsync();
                    await _statisticsDbContext.SaveChangesAsync();

                    return new Either<ActionResult, Unit>(Unit.Instance);
                });
        }

        private async Task<Either<ActionResult, Guid>> GetReleaseId(Guid originalSubjectId, Guid replacementSubjectId)
        {
            // Get the latest Release referencing the original Subject 
            var originalReleaseSubjectId = await (
                    from releaseSubject in _statisticsDbContext.ReleaseSubject
                    join newerVersion in _statisticsDbContext.Release on releaseSubject.ReleaseId equals newerVersion
                        .PreviousVersionId into newerVersionGroup
                    from newerVersion in newerVersionGroup.DefaultIfEmpty()
                    where releaseSubject.SubjectId == originalSubjectId && newerVersion == null
                    select releaseSubject.ReleaseId)
                .SingleAsync();

            // Get the only Release which should be referencing the replacement Subject
            var replacementReleaseId = (await _statisticsDbContext.ReleaseSubject
                    .SingleAsync(releaseSubject => releaseSubject.SubjectId == replacementSubjectId))
                .ReleaseId;

            if (originalReleaseSubjectId != replacementReleaseId)
            {
                return ValidationActionResult(ReplacementDataFileMustBeForSameRelease);
            }

            return originalReleaseSubjectId;
        }

        private ReplacementSubjectMeta GetReplacementSubjectMeta(Guid subjectId)
        {
            var filtersIncludingItems = _filterService.GetFiltersIncludingItems(subjectId)
                .ToList();

            var filters = filtersIncludingItems
                .ToDictionary(filter => filter.Name, filter => filter.Id);

            var filterGroups = filtersIncludingItems.SelectMany(filter => filter.FilterGroups)
                .ToDictionary(filterGroup => filterGroup.Label, filterGroup => filterGroup.Id);

            var filterItems = filtersIncludingItems.SelectMany(filter => filter.FilterGroups)
                .SelectMany(group => group.FilterItems)
                .ToDictionary(filterItem => filterItem.Label, filterItem => filterItem.Id);

            var indicators = _indicatorService.GetIndicators(subjectId)
                .ToDictionary(filterItem => filterItem.Name, filterItem => filterItem.Id);

            var observationalUnits = _locationService.GetObservationalUnits(subjectId);

            var timePeriods = _timePeriodService.GetTimePeriods(subjectId);

            return new ReplacementSubjectMeta
            {
                Filters = filters,
                FilterGroups = filterGroups,
                FilterItems = filterItems,
                Indicators = indicators,
                ObservationalUnits = observationalUnits,
                TimePeriods = timePeriods
            };
        }

        private List<DataBlockReplacementPlanViewModel> ValidateDataBlocks(Guid releaseId, Guid subjectId,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _contentDbContext
                .ReleaseContentBlocks
                .Include(join => join.ContentBlock)
                .Where(join => join.ReleaseId == releaseId)
                .Select(join => join.ContentBlock)
                .OfType<DataBlock>()
                .ToList()
                .Where(dataBlock => dataBlock.Query.SubjectId == subjectId)
                .Select(dataBlock => ValidateDataBlock(dataBlock, replacementSubjectMeta))
                .ToList();
        }

        private DataBlockReplacementPlanViewModel ValidateDataBlock(DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var filterItems = ValidateFilterItemsForDataBlock(dataBlock, replacementSubjectMeta);
            var indicators = ValidateIndicatorsForDataBlock(dataBlock, replacementSubjectMeta);
            var observationalUnits = ValidateObservationalUnitsForDataBlock(dataBlock, replacementSubjectMeta);
            var timePeriods = ValidateTimePeriodsForDataBlock(dataBlock, replacementSubjectMeta);

            return new DataBlockReplacementPlanViewModel(dataBlock.Id,
                dataBlock.Name,
                filterItems,
                indicators,
                observationalUnits,
                timePeriods);
        }

        private List<FootnoteReplacementPlanViewModel> ValidateFootnotes(Guid releaseId, Guid subjectId,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _footnoteService.GetFootnotes(releaseId, subjectId)
                .Select(footnote => ValidateFootnote(footnote, replacementSubjectMeta))
                .ToList();
        }

        private static FootnoteReplacementPlanViewModel ValidateFootnote(Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var filters = ValidateFiltersForFootnote(footnote, replacementSubjectMeta);
            var filterGroups = ValidateFilterGroupsForFootnote(footnote, replacementSubjectMeta);
            var filterItems = ValidateFilterItemsForFootnote(footnote, replacementSubjectMeta);
            var indicators = ValidateIndicatorsForFootnote(footnote, replacementSubjectMeta);

            return new FootnoteReplacementPlanViewModel(footnote.Id,
                footnote.Content,
                filters,
                filterGroups,
                filterItems,
                indicators);
        }

        private static List<FilterReplacementViewModel> ValidateFiltersForFootnote(Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return footnote.Filters
                .Select(filterFootnote => filterFootnote.Filter)
                .Select(filter => ValidateFilterForReplacement(filter, replacementSubjectMeta))
                .ToList();
        }

        private static List<FilterGroupReplacementViewModel> ValidateFilterGroupsForFootnote(Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return footnote.FilterGroups
                .Select(filterGroupFootnote => filterGroupFootnote.FilterGroup)
                .Select(filterGroup => ValidateFilterGroupForReplacement(filterGroup, replacementSubjectMeta))
                .ToList();
        }

        private static List<FilterItemReplacementViewModel> ValidateFilterItemsForFootnote(Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return footnote.FilterItems
                .Select(filterItemFootnote => filterItemFootnote.FilterItem)
                .Select(filterItem => ValidateFilterItemForReplacement(filterItem, replacementSubjectMeta))
                .ToList();
        }

        private static List<IndicatorReplacementViewModel> ValidateIndicatorsForFootnote(Footnote footnote,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return footnote.Indicators
                .Select(indicatorFootnote => indicatorFootnote.Indicator)
                .Select(indicator => ValidateIndicatorForReplacement(indicator, replacementSubjectMeta))
                .ToList();
        }

        private List<FilterItemReplacementViewModel> ValidateFilterItemsForDataBlock(DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _statisticsDbContext.FilterItem
                .Where(filterItem => dataBlock.Query.Filters.Contains(filterItem.Id))
                .Select(filterItem => ValidateFilterItemForReplacement(filterItem, replacementSubjectMeta))
                .ToList();
        }

        private List<IndicatorReplacementViewModel> ValidateIndicatorsForDataBlock(DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _statisticsDbContext.Indicator
                .Where(indicator => dataBlock.Query.Indicators.Contains(indicator.Id))
                .Select(indicator => ValidateIndicatorForReplacement(indicator, replacementSubjectMeta))
                .ToList();
        }

        private static Dictionary<string, ObservationalUnitReplacementViewModel> ValidateObservationalUnitsForDataBlock(
            DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return GetEnumValues<GeographicLevel>()
                .Where(geographicLevel => !IgnoredLevels.Contains(geographicLevel))
                .ToDictionary(geographicLevel => geographicLevel.ToString(),
                    geographicLevel =>
                        ValidateObservationalUnitsForReplacement(dataBlock.Query.Locations,
                            geographicLevel,
                            replacementSubjectMeta)
                );
        }

        private static TimePeriodReplacementViewModel ValidateTimePeriodsForDataBlock(DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var range = TimePeriodUtil.Range(dataBlock.Query.TimePeriod);
            return new TimePeriodReplacementViewModel
            {
                Query = dataBlock.Query.TimePeriod,
                Valid = range.Intersect(replacementSubjectMeta.TimePeriods).Any()
            };
        }

        private static FilterReplacementViewModel ValidateFilterForReplacement(Filter filter,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            Guid? target = replacementSubjectMeta.Filters.GetValueOrDefault(filter.Name);
            target = target == Guid.Empty ? null : target;
            return new FilterReplacementViewModel
            {
                Id = filter.Id,
                Name = filter.Name,
                Label = filter.Label,
                Target = target
            };
        }

        private static FilterGroupReplacementViewModel ValidateFilterGroupForReplacement(FilterGroup filterGroup,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            Guid? target = replacementSubjectMeta.FilterGroups.GetValueOrDefault(filterGroup.Label);
            target = target == Guid.Empty ? null : target;
            return new FilterGroupReplacementViewModel
            {
                Id = filterGroup.Id,
                Label = filterGroup.Label,
                FilterLabel = filterGroup.Filter.Label,
                Target = target
            };
        }

        private static FilterItemReplacementViewModel ValidateFilterItemForReplacement(FilterItem filterItem,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            Guid? target = replacementSubjectMeta.FilterItems.GetValueOrDefault(filterItem.Label);
            target = target == Guid.Empty ? null : target;
            return new FilterItemReplacementViewModel
            {
                Id = filterItem.Id,
                Label = filterItem.Label,
                Target = target
            };
        }

        private static IndicatorReplacementViewModel ValidateIndicatorForReplacement(Indicator indicator,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            Guid? target = replacementSubjectMeta.Indicators.GetValueOrDefault(indicator.Name);
            target = target == Guid.Empty ? null : target;
            return new IndicatorReplacementViewModel
            {
                Id = indicator.Id,
                Name = indicator.Name,
                Label = indicator.Label,
                Target = target
            };
        }

        private static ObservationalUnitReplacementViewModel ValidateObservationalUnitsForReplacement(
            LocationQuery locationQuery,
            GeographicLevel geographicLevel,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            var queryProperty = typeof(LocationQuery).GetProperty(geographicLevel.ToString());
            if (queryProperty == null || queryProperty.GetMethod == null)
            {
                throw new ArgumentException(
                    $"{nameof(locationQuery)} does not have a property {geographicLevel.ToString()} with get method");
            }

            var originalCodes =
                queryProperty.GetMethod.Invoke(locationQuery, new object[] { }) as IEnumerable<string> ??
                new List<string>();

            var replacementCodes = replacementSubjectMeta.ObservationalUnits.GetValueOrDefault(geographicLevel)
                ?.Select(unit => unit.Code)
                .ToList() ?? new List<string>();

            return new ObservationalUnitReplacementViewModel
            {
                Matched = originalCodes.Intersect(replacementCodes),
                Unmatched = originalCodes.Except(replacementCodes),
            };
        }

        private async Task ReplaceLinksForDataBlock(DataBlockReplacementPlanViewModel replacementPlan, Guid replacementSubjectId)
        {
            var dataBlock = await _contentDbContext.ContentBlocks
                .OfType<DataBlock>()
                .SingleAsync(block => block.Id == replacementPlan.Id);

            _contentDbContext.Update(dataBlock);
            
            dataBlock.Query.SubjectId = replacementSubjectId;
            ReplaceDataBlockQueryFilters(replacementPlan, dataBlock);
            ReplaceDataBlockQueryIndicators(replacementPlan, dataBlock);
            ReplaceDataBlockTableHeaders(replacementPlan, dataBlock);
        }

        private static void ReplaceDataBlockQueryFilters(DataBlockReplacementPlanViewModel replacementPlan,
            DataBlock dataBlock)
        {
            var filterItems = dataBlock.Query.Filters.ToList();

            replacementPlan.FilterItems.ToList().ForEach(plan =>
            {
                filterItems.Remove(plan.Id);
                filterItems.Add(plan.TargetValue);
            });
            
            dataBlock.Query.Filters = filterItems;
        }

        private static void ReplaceDataBlockQueryIndicators(DataBlockReplacementPlanViewModel replacementPlan,
            DataBlock dataBlock)
        {
            var indicators = dataBlock.Query.Indicators.ToList();
            
            replacementPlan.Indicators.ToList().ForEach(plan =>
            {
                indicators.Remove(plan.Id);
                indicators.Add(plan.TargetValue);
            });
            
            dataBlock.Query.Indicators = indicators;
        }
        
        private static void ReplaceDataBlockTableHeaders(DataBlockReplacementPlanViewModel replacementPlan,
            DataBlock dataBlock)
        {
            var tableHeaders = dataBlock.Table.TableHeaders;
            var tableHeaderColumns = tableHeaders.Columns.ToList();
            var tableHeaderRows = tableHeaders.Rows.ToList();

            var filterItemTargets = replacementPlan.FilterItems.ToDictionary(plan => plan.Id, plan => plan.TargetValue);
            var indicatorTargets = replacementPlan.Indicators.ToDictionary(plan => plan.Id, plan => plan.TargetValue);

            ReplaceDataBlockTableHeaders(
                tableHeaderColumns.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaderRows.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaderColumns.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaderRows.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);

            dataBlock.Table.TableHeaders.Columns = tableHeaderColumns;
            dataBlock.Table.TableHeaders.Rows = tableHeaderRows;
            
            // TODO EES- ColGroups
            // TODO RowGroups
        }
        
        private static void ReplaceDataBlockTableHeaders(List<TableHeader> tableHeaders, DataBlock dataBlock,
            IReadOnlyDictionary<Guid, Guid> targets)
        {
            foreach (var tableHeader in tableHeaders)
            {
                if (Guid.TryParse(tableHeader.Value, out var idAsGuid))
                {
                    if (targets.TryGetValue(idAsGuid, out var targetId))
                    {
                        tableHeader.Value = targetId.ToString();
                    }
                    else
                    {
                        throw new InvalidOperationException($"Expected target replacement value for dataBlock ${dataBlock.Id} ${tableHeader.Type} table header value: ${idAsGuid}");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Expected Guid for dataBlock ${dataBlock.Id} ${tableHeader.Type} table header value but found: ${tableHeader.Value}");
                }   
            }
        }  

        private async Task ReplaceLinksForFootnote(FootnoteReplacementPlanViewModel replacementPlan)
        {
            await replacementPlan.Filters.ForEachAsync(async plan =>
                await ReplaceFootnoteFilter(replacementPlan.Id, plan));

            await replacementPlan.FilterGroups.ForEachAsync(async plan =>
                await ReplaceFootnoteFilterGroup(replacementPlan.Id, plan));

            await replacementPlan.FilterItems.ForEachAsync(async plan =>
                await ReplaceFootnoteFilterItem(replacementPlan.Id, plan));

            await replacementPlan.Indicators.ForEachAsync(async plan =>
                await ReplaceIndicatorFootnote(replacementPlan.Id, plan));
        }

        private async Task ReplaceFootnoteFilter(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterFootnote = await _statisticsDbContext.FilterFootnote.SingleAsync(f =>
                f.FootnoteId == footnoteId && f.FilterId == plan.Id
            );

            _statisticsDbContext.Update(filterFootnote);
            filterFootnote.FilterId = plan.TargetValue;
        }

        private async Task ReplaceFootnoteFilterGroup(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            if (!plan.Target.HasValue)
            {
                throw new ArgumentException($"{nameof(plan)} does not have a target replacement value");
            }

            var filterGroupFootnote = await _statisticsDbContext.FilterGroupFootnote.SingleAsync(f =>
                f.FootnoteId == footnoteId && f.FilterGroupId == plan.Id
            );

            _statisticsDbContext.Update(filterGroupFootnote);
            filterGroupFootnote.FilterGroupId = plan.TargetValue;
        }

        private async Task ReplaceFootnoteFilterItem(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterItemFootnote = await _statisticsDbContext.FilterItemFootnote.SingleAsync(f =>
                f.FootnoteId == footnoteId && f.FilterItemId == plan.Id
            );

            _statisticsDbContext.Update(filterItemFootnote);
            filterItemFootnote.FilterItemId = plan.TargetValue;
        }

        private async Task ReplaceIndicatorFootnote(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var indicatorFootnote = await _statisticsDbContext.IndicatorFootnote.SingleAsync(f =>
                f.FootnoteId == footnoteId && f.IndicatorId == plan.Id
            );

            _statisticsDbContext.Update(indicatorFootnote);
            indicatorFootnote.IndicatorId = plan.TargetValue;
        }

        private class ReplacementSubjectMeta
        {
            public Dictionary<string, Guid> Filters { get; set; }
            public Dictionary<string, Guid> FilterGroups { get; set; }
            public Dictionary<string, Guid> FilterItems { get; set; }
            public Dictionary<string, Guid> Indicators { get; set; }
            public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> ObservationalUnits { get; set; }
            public IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> TimePeriods { get; set; }
        }
    }
}