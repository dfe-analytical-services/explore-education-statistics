using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
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

        public async Task<Either<ActionResult, ReplacementPlan>> GetReplacementPlan(Guid originalSubjectId,
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

                    return new ReplacementPlan(dataBlocks, footnotes);
                });
        }

        private async Task<Either<ActionResult, Guid>> GetReleaseId(Guid originalSubjectId, Guid replacementSubjectId)
        {
            var originalReleaseSubject = await _statisticsDbContext.ReleaseSubject
                .FirstAsync(releaseSubject => releaseSubject.SubjectId == originalSubjectId);

            var replacementReleaseSubject = await _statisticsDbContext.ReleaseSubject
                .FirstAsync(releaseSubject => releaseSubject.SubjectId == replacementSubjectId);

            if (originalReleaseSubject.ReleaseId != replacementReleaseSubject.ReleaseId)
            {
                return ValidationActionResult(ReplacementDataFileMustBeForSameRelease);
            }

            return originalReleaseSubject.ReleaseId;
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

        private List<DataBlockReplacementPlan> ValidateDataBlocks(Guid releaseId, Guid subjectId,
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

        private DataBlockReplacementPlan ValidateDataBlock(DataBlock dataBlock, ReplacementSubjectMeta replacementSubjectMeta)
        {
            var filterItems = ValidateFilterItemsForDataBlock(dataBlock, replacementSubjectMeta);
            var indicators = ValidateIndicatorsForDataBlock(dataBlock, replacementSubjectMeta);
            var observationalUnits = ValidateObservationalUnitsForDataBlock(dataBlock, replacementSubjectMeta);
            var timePeriods = ValidateTimePeriodsForDataBlock(dataBlock, replacementSubjectMeta);

            return new DataBlockReplacementPlan(dataBlock.Id,
                dataBlock.Name,
                filterItems,
                indicators,
                observationalUnits,
                timePeriods);
        }

        private List<FootnoteReplacementPlan> ValidateFootnotes(Guid releaseId, Guid subjectId,
            ReplacementSubjectMeta replacementSubjectMeta)
        {
            return _footnoteService.GetFootnotes(releaseId, subjectId)
                .Select(footnote => ValidateFootnote(footnote, replacementSubjectMeta))
                .ToList();
        }

        private static FootnoteReplacementPlan ValidateFootnote(Footnote footnote, ReplacementSubjectMeta replacementSubjectMeta)
        {
            var filters = ValidateFiltersForFootnote(footnote, replacementSubjectMeta);
            var filterGroups = ValidateFilterGroupsForFootnote(footnote, replacementSubjectMeta);
            var filterItems = ValidateFilterItemsForFootnote(footnote, replacementSubjectMeta);
            var indicators = ValidateIndicatorsForFootnote(footnote, replacementSubjectMeta);

            return new FootnoteReplacementPlan(footnote.Id,
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
                    $"{nameof(LocationQuery)} does not have a property {geographicLevel.ToString()} with get method");
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