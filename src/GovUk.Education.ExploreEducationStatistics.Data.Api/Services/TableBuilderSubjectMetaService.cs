using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class TableBuilderSubjectMetaService : ITableBuilderSubjectMetaService
    {
        private readonly IFilterItemService _filterItemService;
        private readonly IIndicatorGroupService _indicatorGroupService;
        private readonly ILocationService _locationService;
        private readonly IMapper _mapper;
        private readonly IObservationService _observationService;
        private readonly ISubjectService _subjectService;

        public TableBuilderSubjectMetaService(IFilterItemService filterItemService,
            IIndicatorGroupService indicatorGroupService,
            ILocationService locationService,
            IMapper mapper,
            IObservationService observationService,
            ISubjectService subjectService)
        {
            _filterItemService = filterItemService;
            _indicatorGroupService = indicatorGroupService;
            _locationService = locationService;
            _mapper = mapper;
            _observationService = observationService;
            _subjectService = subjectService;
        }

        public TableBuilderSubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query)
        {
            var subject = _subjectService.Find(query.SubjectId);
            if (subject == null)
            {
                throw new ArgumentException("Subject does not exist", nameof(query.SubjectId));
            }

            return new TableBuilderSubjectMetaViewModel
            {
                Filters = GetFilters(query),
                Indicators = GetIndicators(subject.Id),
                Locations = GetObservationalUnits(query),
                TimePeriod = GetTimePeriods(query)
            };
        }

        private TableBuilderTimePeriodsMetaViewModel GetTimePeriods(
            SubjectMetaQueryContext query)
        {
            var timePeriodsMeta = _observationService.GetTimePeriodsMeta(query);

            return new TableBuilderTimePeriodsMetaViewModel
            {
                Hint = "Filter statistics by a given start and end date",
                Legend = "Academic Year",
                Options = timePeriodsMeta.Select(tuple => new TimePeriodMetaViewModel
                {
                    Code = tuple.TimeIdentifier,
                    Label = TimePeriodLabelFormatter.Format(tuple.Year, tuple.TimeIdentifier),
                    Year = tuple.Year
                })
            };
        }

        private Dictionary<string, TableBuilderObservationalUnitsMetaViewModel> GetObservationalUnits(
            SubjectMetaQueryContext query)
        {
            var observationalUnits = _locationService.GetObservationalUnits(query.ObservationPredicate());
            return observationalUnits.ToDictionary(
                pair => pair.Key.ToString().CamelCase(),
                pair => new TableBuilderObservationalUnitsMetaViewModel
                {
                    Hint = "",
                    Legend = pair.Key.GetEnumLabel(),
                    Options = _mapper.Map<IEnumerable<LabelValueViewModel>>(pair.Value)
                });
        }

        private Dictionary<string, TableBuilderIndicatorsMetaViewModel> GetIndicators(
            long subjectId)
        {
            return _indicatorGroupService.GetIndicatorGroups(subjectId).ToDictionary(
                group => group.Label.PascalCase(),
                group => new TableBuilderIndicatorsMetaViewModel
                {
                    Label = group.Label,
                    Options = _mapper.Map<IEnumerable<IndicatorMetaViewModel>>(group.Indicators)
                }
            );
        }

        private Dictionary<string, TableBuilderFilterMetaViewModel> GetFilters(
            SubjectMetaQueryContext query)
        {
            return _filterItemService.GetFilterItems(query.ObservationPredicate())
                .GroupBy(item => item.FilterGroup.Filter)
                .ToDictionary(
                    itemsGroupedByFilter => itemsGroupedByFilter.Key.Label.PascalCase(),
                    itemsGroupedByFilter => new TableBuilderFilterMetaViewModel
                    {
                        Hint = itemsGroupedByFilter.Key.Hint,
                        Legend = itemsGroupedByFilter.Key.Label,
                        Options = itemsGroupedByFilter.GroupBy(item => item.FilterGroup).ToDictionary(
                            itemsGroupedByFilterGroup => itemsGroupedByFilterGroup.Key.Label.PascalCase(),
                            itemsGroupedByFilterGroup =>
                                new TableBuilderFilterItemMetaViewModel
                                {
                                    Label = itemsGroupedByFilterGroup.Key.Label,
                                    Options = itemsGroupedByFilterGroup.Select(item => new LabelValueViewModel
                                    {
                                        Label = item.Label,
                                        Value = item.Id.ToString()
                                    })
                                }),
                        TotalValue = GetTotalValue(itemsGroupedByFilter)
                    });
        }

        private static string GetTotalValue(IEnumerable<FilterItem> filterItems)
        {
            return GetTotalGroup(filterItems)?.FirstOrDefault(IsFilterItemTotal)?.Id.ToString() ?? string.Empty;
        }

        private static IEnumerable<FilterItem> GetTotalGroup(IEnumerable<FilterItem> filterItems)
        {
            var itemsGroupedByFilterGroup = filterItems.GroupBy(item => item.FilterGroup).ToList();
            // Return the 'Default' group if there is only one, otherwise the 'Total' group if it exists
            return itemsGroupedByFilterGroup.Count == 1 ?
                itemsGroupedByFilterGroup.First(items => IsFilterGroupDefault(items.Key)) :
                itemsGroupedByFilterGroup.FirstOrDefault(items => IsFilterGroupTotal(items.Key));
        }

        private static bool IsFilterItemTotal(FilterItem item)
        {
            return IsEqualToIgnoreCase(item.Label, "Total");
        }

        private static bool IsFilterGroupDefault(FilterGroup group)
        {
            return IsEqualToIgnoreCase(group.Label, "Default");
        }

        private static bool IsFilterGroupTotal(FilterGroup group)
        {
            return IsEqualToIgnoreCase(group.Label, "Total");
        }

        private static bool IsEqualToIgnoreCase(string value, string compareTo)
        {
            return value.Equals(compareTo, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}