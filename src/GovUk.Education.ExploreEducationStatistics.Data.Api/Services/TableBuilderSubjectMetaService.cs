using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
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
        private readonly ITimePeriodService _timePeriodService;

        public TableBuilderSubjectMetaService(
            IFilterItemService filterItemService,
            IIndicatorGroupService indicatorGroupService,
            ILocationService locationService,
            IMapper mapper,
            IObservationService observationService,
            ISubjectService subjectService,
            ITimePeriodService timePeriodService)
        {
            _filterItemService = filterItemService;
            _indicatorGroupService = indicatorGroupService;
            _locationService = locationService;
            _mapper = mapper;
            _observationService = observationService;
            _subjectService = subjectService;
            _timePeriodService = timePeriodService;
        }

        public TableBuilderSubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query)
        {
            var observations = _observationService.FindObservations(query).AsQueryable();
            var subject = _subjectService.Find(query.SubjectId);
            if (subject == null)
            {
                throw new ArgumentException("Subject does not exist", nameof(query.SubjectId));
            }

            return new TableBuilderSubjectMetaViewModel
            {
                Filters = GetFilters(observations),
                Indicators = GetIndicators(subject.Id),
                Locations = GetObservationalUnits(observations),
                TimePeriod = GetTimePeriods(observations)
            };
        }

        private TableBuilderTimePeriodsMetaViewModel GetTimePeriods(IQueryable<Observation> observations)
        {
            var timePeriods = _timePeriodService.GetTimePeriods(observations)
                .Select(tuple => new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));

            return new TableBuilderTimePeriodsMetaViewModel
            {
                Hint = "Filter statistics by a given start and end date",
                Legend = "",
                Options = timePeriods
            };
        }

        private Dictionary<string, TableBuilderObservationalUnitsMetaViewModel> GetObservationalUnits(
            IQueryable<Observation> observations)
        {
            var observationalUnits = _locationService.GetObservationalUnits(observations);
            return observationalUnits.ToDictionary(
                pair => pair.Key.ToString().CamelCase(),
                pair => new TableBuilderObservationalUnitsMetaViewModel
                {
                    Hint = "",
                    Legend = pair.Key.GetEnumLabel(),
                    Options = _mapper.Map<IEnumerable<LabelValue>>(pair.Value)
                });
        }

        private Dictionary<string, TableBuilderIndicatorsMetaViewModel> GetIndicators(long subjectId)
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

        private Dictionary<string, TableBuilderFilterMetaViewModel> GetFilters(IQueryable<Observation> observations)
        {
            return _filterItemService.GetFilterItemsIncludingFilters(observations)
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
                                new TableBuilderFilterItemsMetaViewModel
                                {
                                    Label = itemsGroupedByFilterGroup.Key.Label,
                                    Options = itemsGroupedByFilterGroup.Select(item => new LabelValue
                                    {
                                        Label = item.Label,
                                        Value = item.Id.ToString()
                                    })
                                }),
                        TotalValue = GetTotalValue(itemsGroupedByFilter)
                    });
        }

        private string GetTotalValue(IEnumerable<FilterItem> filterItems)
        {
            return _filterItemService.GetTotal(filterItems)?.Id.ToString() ?? string.Empty;
        }
    }
}