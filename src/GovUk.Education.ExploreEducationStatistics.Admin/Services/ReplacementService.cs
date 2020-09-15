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
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
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
        private readonly IReleaseService _releaseService;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IPersistenceHelper<ContentDbContext> _contentPersistenceHelper;

        public ReplacementService(ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            IFilterService filterService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IFootnoteService footnoteService,
            IReleaseService releaseService,
            ITimePeriodService timePeriodService,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper)
        {
            _contentDbContext = contentDbContext;
            _statisticsDbContext = statisticsDbContext;
            _filterService = filterService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _footnoteService = footnoteService;
            _releaseService = releaseService;
            _timePeriodService = timePeriodService;
            _contentPersistenceHelper = contentPersistenceHelper;
        }

        public async Task<Either<ActionResult, ReplacementPlanViewModel>> GetReplacementPlan(
            Guid originalReleaseFileReferenceId,
            Guid replacementReleaseFileReferenceId)
        {
            return await CheckReleaseFileReferenceExists(originalReleaseFileReferenceId)
                .OnSuccess(async originalReleaseFileReference =>
                {
                    return await CheckReleaseFileReferenceExists(replacementReleaseFileReferenceId)
                        .OnSuccessDo(replacementReleaseFileReference =>
                            CheckFilesAreForRelatedReleases(originalReleaseFileReference,
                                replacementReleaseFileReference))
                        .OnSuccess(replacementReleaseFileReference =>
                        {
                            var releaseId = replacementReleaseFileReference.ReleaseId;
                            var originalSubjectId = originalReleaseFileReference.SubjectId.Value;
                            var replacementSubjectId = replacementReleaseFileReference.SubjectId.Value;

                            var replacementSubjectMeta = GetReplacementSubjectMeta(replacementSubjectId);

                            var dataBlocks = ValidateDataBlocks(releaseId, originalSubjectId, replacementSubjectMeta);
                            var footnotes = ValidateFootnotes(releaseId, originalSubjectId, replacementSubjectMeta);

                            return new ReplacementPlanViewModel(dataBlocks, footnotes, originalSubjectId,
                                replacementSubjectId);
                        });
                });
        }

        public async Task<Either<ActionResult, Unit>> Replace(Guid originalReleaseFileReferenceId,
            Guid replacementReleaseFileReferenceId)
        {
            return await GetReplacementPlan(originalReleaseFileReferenceId, replacementReleaseFileReferenceId)
                .OnSuccess(async replacementPlan =>
                {
                    if (!replacementPlan.Valid)
                    {
                        return ValidationActionResult(ReplacementMustBeValid);
                    }

                    await replacementPlan.DataBlocks.ForEachAsync(plan =>
                        ReplaceLinksForDataBlock(plan, replacementPlan.ReplacementSubjectId));
                    await replacementPlan.Footnotes.ForEachAsync(plan =>
                        ReplaceLinksForFootnote(plan, replacementPlan.OriginalSubjectId,
                            replacementPlan.ReplacementSubjectId));

                    await _contentDbContext.SaveChangesAsync();
                    await _statisticsDbContext.SaveChangesAsync();

                    // This Release Id can be found on the ReplacementFileReference
                    var releaseId = (await _contentDbContext.ReleaseFileReferences
                        .FindAsync(replacementReleaseFileReferenceId)).ReleaseId;

                    return await RemoveSubjectAndFileFromRelease(releaseId, replacementPlan.OriginalSubjectId,
                        originalReleaseFileReferenceId);
                });
        }

        private async Task<Either<ActionResult, ReleaseFileReference>> CheckReleaseFileReferenceExists(Guid id)
        {
            return await _contentPersistenceHelper.CheckEntityExists<ReleaseFileReference>(id)
                .OnSuccess(releaseFileReference => releaseFileReference.ReleaseFileType != ReleaseFileTypes.Data
                    ? new Either<ActionResult, ReleaseFileReference>(
                        ValidationActionResult(ReplacementFileTypesMustBeData))
                    : releaseFileReference);
        }

        private async Task<Either<ActionResult, Unit>> CheckFilesAreForRelatedReleases(
            ReleaseFileReference originalReleaseFileReference,
            ReleaseFileReference replacementReleaseFileReference)
        {
            // Get the latest Release referencing the original ReleaseFileReference
            var originalReleaseId = await (
                    from releaseFile in _contentDbContext.ReleaseFiles
                    join newerVersion in _contentDbContext.Releases on releaseFile.ReleaseId equals newerVersion
                        .PreviousVersionId into newerVersionGroup
                    from newerVersion in newerVersionGroup.DefaultIfEmpty()
                    where releaseFile.ReleaseFileReferenceId == originalReleaseFileReference.Id && newerVersion == null
                    select releaseFile.ReleaseId)
                .SingleAsync();

            // Check the replacement is for the same Release
            if (replacementReleaseFileReference.ReleaseId != originalReleaseId)
            {
                return ValidationActionResult(ReplacementDataFileMustBeForRelatedRelease);
            }

            return Unit.Instance;
        }

        private ReplacementSubjectMeta GetReplacementSubjectMeta(Guid subjectId)
        {
            var filtersIncludingItems = _filterService.GetFiltersIncludingItems(subjectId)
                .ToList();

            var filters = filtersIncludingItems
                .ToDictionary(filter => filter.Name, filter => filter);

            var indicators = _indicatorService.GetIndicators(subjectId)
                .ToDictionary(filterItem => filterItem.Name, filterItem => filterItem);

            var observationalUnits = _locationService.GetObservationalUnits(subjectId);

            var timePeriods = _timePeriodService.GetTimePeriods(subjectId);

            return new ReplacementSubjectMeta
            {
                Filters = filters,
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
            var locations = ValidateLocationsForDataBlock(dataBlock, replacementSubjectMeta);
            var timePeriods = ValidateTimePeriodsForDataBlock(dataBlock, replacementSubjectMeta);

            return new DataBlockReplacementPlanViewModel(dataBlock.Id,
                dataBlock.Name,
                filterItems,
                indicators,
                locations,
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
                .Include(filterItem => filterItem.FilterGroup)
                .ThenInclude(filterGroup => filterGroup.Filter)
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

        private static Dictionary<string, LocationReplacementViewModel> ValidateLocationsForDataBlock(
            DataBlock dataBlock,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return GetEnumValues<GeographicLevel>()
                .Where(geographicLevel => !IgnoredLevels.Contains(geographicLevel))
                .ToDictionary(geographicLevel => geographicLevel.ToString(),
                    geographicLevel =>
                        ValidateLocationsForReplacement(dataBlock.Query.Locations,
                            geographicLevel,
                            replacementSubjectMeta))
                .Filter(pair => pair.Value.Any);
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
            return new FilterReplacementViewModel
            {
                Id = filter.Id,
                Name = filter.Name,
                Label = filter.Label,
                Target = FindReplacementFilter(replacementSubjectMeta,
                    filter.Name)?.Id
            };
        }

        private static FilterGroupReplacementViewModel ValidateFilterGroupForReplacement(FilterGroup filterGroup,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return new FilterGroupReplacementViewModel
            {
                Id = filterGroup.Id,
                Label = filterGroup.Label,
                FilterLabel = filterGroup.Filter.Label,
                Target = FindReplacementFilterGroup(replacementSubjectMeta,
                    filterGroup.Filter.Name,
                    filterGroup.Label)?.Id
            };
        }

        private static FilterItemReplacementViewModel ValidateFilterItemForReplacement(FilterItem filterItem,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return new FilterItemReplacementViewModel
            {
                Id = filterItem.Id,
                Label = filterItem.Label,
                Target = FindReplacementFilterItem(replacementSubjectMeta,
                    filterItem.FilterGroup.Filter.Name,
                    filterItem.FilterGroup.Label,
                    filterItem.Label)?.Id
            };
        }

        private static IndicatorReplacementViewModel ValidateIndicatorForReplacement(Indicator indicator,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return new IndicatorReplacementViewModel
            {
                Id = indicator.Id,
                Name = indicator.Name,
                Label = indicator.Label,
                Target = FindReplacementIndicator(replacementSubjectMeta,
                    indicator.Name)
            };
        }

        private static LocationReplacementViewModel ValidateLocationsForReplacement(
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

            return new LocationReplacementViewModel
            {
                Matched = originalCodes.Intersect(replacementCodes),
                Unmatched = originalCodes.Except(replacementCodes),
            };
        }

        private static Filter FindReplacementFilter(ReplacementSubjectMeta replacementSubjectMeta,
            string filterName)
        {
            return replacementSubjectMeta.Filters.GetValueOrDefault(filterName);
        }

        private static FilterGroup FindReplacementFilterGroup(ReplacementSubjectMeta replacementSubjectMeta,
            string filterName,
            string filterGroupLabel)
        {
            var replacementFilter = FindReplacementFilter(replacementSubjectMeta, filterName);
            return replacementFilter?.FilterGroups.SingleOrDefault(filterGroup =>
                filterGroup.Label == filterGroupLabel);
        }

        private static FilterItem FindReplacementFilterItem(ReplacementSubjectMeta replacementSubjectMeta,
            string filterName,
            string filterGroupLabel,
            string filterItemLabel)
        {
            var replacementFilterGroup =
                FindReplacementFilterGroup(replacementSubjectMeta, filterName, filterGroupLabel);
            return replacementFilterGroup?.FilterItems.SingleOrDefault(filterItem =>
                filterItem.Label == filterItemLabel);
        }

        private static Guid? FindReplacementIndicator(ReplacementSubjectMeta replacementSubjectMeta,
            string indicatorName)
        {
            return replacementSubjectMeta.Indicators.GetValueOrDefault(indicatorName)?.Id;
        }

        private async Task ReplaceLinksForDataBlock(DataBlockReplacementPlanViewModel replacementPlan,
            Guid replacementSubjectId)
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

            var filterItemTargets = replacementPlan.FilterItems.ToDictionary(plan => plan.Id, plan => plan.TargetValue);
            var indicatorTargets = replacementPlan.Indicators.ToDictionary(plan => plan.Id, plan => plan.TargetValue);

            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Columns.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);

            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);
            ReplaceDataBlockTableHeaders(
                tableHeaders.Rows.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);

            foreach (var group in tableHeaders.ColumnGroups)
            {
                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);
            }

            foreach (var group in tableHeaders.RowGroups)
            {
                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Filter), dataBlock, filterItemTargets);

                ReplaceDataBlockTableHeaders(
                    group.FilterByType(TableHeaderType.Indicator), dataBlock, indicatorTargets);
            }
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
                        throw new InvalidOperationException(
                            $"Expected target replacement value for dataBlock {dataBlock.Id} {tableHeader.Type} table header value: {idAsGuid}");
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Expected Guid for dataBlock {dataBlock.Id} {tableHeader.Type} table header value but found: {tableHeader.Value}");
                }
            }
        }

        private async Task ReplaceLinksForFootnote(FootnoteReplacementPlanViewModel replacementPlan,
            Guid originalSubjectId,
            Guid replacementSubjectId)
        {
            await ReplaceFootnoteSubject(replacementPlan.Id, originalSubjectId, replacementSubjectId);

            await replacementPlan.Filters.ForEachAsync(async plan =>
                await ReplaceFootnoteFilter(replacementPlan.Id, plan));

            await replacementPlan.FilterGroups.ForEachAsync(async plan =>
                await ReplaceFootnoteFilterGroup(replacementPlan.Id, plan));

            await replacementPlan.FilterItems.ForEachAsync(async plan =>
                await ReplaceFootnoteFilterItem(replacementPlan.Id, plan));

            await replacementPlan.Indicators.ForEachAsync(async plan =>
                await ReplaceIndicatorFootnote(replacementPlan.Id, plan));
        }

        private async Task ReplaceFootnoteSubject(Guid footnoteId, Guid originalSubjectId, Guid replacementSubjectId)
        {
            var subjectFootnote = await _statisticsDbContext.SubjectFootnote.Where(f =>
                f.FootnoteId == footnoteId && f.SubjectId == originalSubjectId).SingleOrDefaultAsync();

            if (subjectFootnote != null)
            {
                _statisticsDbContext.Remove(subjectFootnote);
                await _statisticsDbContext.SubjectFootnote.AddAsync(new SubjectFootnote
                {
                    FootnoteId = footnoteId,
                    SubjectId = replacementSubjectId
                });
            }
        }

        private async Task ReplaceFootnoteFilter(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterFootnote = await _statisticsDbContext.FilterFootnote.SingleAsync(f =>
                f.FootnoteId == footnoteId && f.FilterId == plan.Id
            );

            _statisticsDbContext.Remove(filterFootnote);
            await _statisticsDbContext.FilterFootnote.AddAsync(new FilterFootnote
            {
                FootnoteId = footnoteId,
                FilterId = plan.TargetValue
            });
        }

        private async Task ReplaceFootnoteFilterGroup(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterGroupFootnote = await _statisticsDbContext.FilterGroupFootnote.SingleAsync(f =>
                f.FootnoteId == footnoteId && f.FilterGroupId == plan.Id
            );

            _statisticsDbContext.Remove(filterGroupFootnote);
            await _statisticsDbContext.FilterGroupFootnote.AddAsync(new FilterGroupFootnote
            {
                FootnoteId = footnoteId,
                FilterGroupId = plan.TargetValue
            });
        }

        private async Task ReplaceFootnoteFilterItem(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var filterItemFootnote = await _statisticsDbContext.FilterItemFootnote.SingleAsync(f =>
                f.FootnoteId == footnoteId && f.FilterItemId == plan.Id
            );

            _statisticsDbContext.Remove(filterItemFootnote);
            await _statisticsDbContext.FilterItemFootnote.AddAsync(new FilterItemFootnote
            {
                FootnoteId = footnoteId,
                FilterItemId = plan.TargetValue
            });
        }

        private async Task ReplaceIndicatorFootnote(Guid footnoteId, TargetableReplacementViewModel plan)
        {
            var indicatorFootnote = await _statisticsDbContext.IndicatorFootnote.SingleAsync(f =>
                f.FootnoteId == footnoteId && f.IndicatorId == plan.Id
            );

            _statisticsDbContext.Remove(indicatorFootnote);
            await _statisticsDbContext.IndicatorFootnote.AddAsync(new IndicatorFootnote
            {
                FootnoteId = footnoteId,
                IndicatorId = plan.TargetValue
            });
        }

        private async Task<Either<ActionResult, Unit>> RemoveSubjectAndFileFromRelease(Guid releaseId,
            Guid subjectId,
            Guid releaseFileReferenceId)
        {
            var releaseFileReference = await _contentDbContext.ReleaseFileReferences
                .FindAsync(releaseFileReferenceId);

            var subject =
                await _statisticsDbContext.Subject.FindAsync(subjectId);

            return await _releaseService.RemoveDataFilesAsync(releaseId,
                releaseFileReference.Filename, subject.Name);
        }

        private class ReplacementSubjectMeta
        {
            public Dictionary<string, Filter> Filters { get; set; }
            public Dictionary<string, Indicator> Indicators { get; set; }
            public Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> ObservationalUnits { get; set; }
            public IEnumerable<(int Year, TimeIdentifier TimeIdentifier)> TimePeriods { get; set; }
        }
    }
}