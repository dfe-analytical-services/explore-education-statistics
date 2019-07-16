using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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

        private LegendOptionsMetaValueModel<IEnumerable<TimePeriodMetaViewModel>> GetTimePeriods(
            SubjectMetaQueryContext query)
        {
            var timePeriodsMeta = _observationService.GetTimePeriodsMeta(query);

            return new LegendOptionsMetaValueModel<IEnumerable<TimePeriodMetaViewModel>>
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

        private Dictionary<string, LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>> GetObservationalUnits(
            SubjectMetaQueryContext query)
        {
            var observationalUnits = _locationService.GetObservationalUnits(query.ObservationPredicate());
            return observationalUnits.ToDictionary(
                pair => pair.Key.ToString().CamelCase(),
                pair => new LegendOptionsMetaValueModel<IEnumerable<LabelValueViewModel>>
                {
                    Hint = "",
                    Legend = pair.Key.GetEnumLabel(),
                    Options = _mapper.Map<IEnumerable<LabelValueViewModel>>(pair.Value)
                });
        }

        private Dictionary<string, LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>> GetIndicators(
            long subjectId)
        {
            return _indicatorGroupService.GetIndicatorGroups(subjectId).ToDictionary(
                group => group.Label.PascalCase(),
                group => new LabelOptionsMetaValueModel<IEnumerable<IndicatorMetaViewModel>>
                {
                    Label = group.Label,
                    Options = _mapper.Map<IEnumerable<IndicatorMetaViewModel>>(group.Indicators)
                }
            );
        }

        private Dictionary<string, LegendOptionsMetaValueModel<Dictionary<string, FilterItemMetaViewModel>>> GetFilters(
            SubjectMetaQueryContext query)
        {
            return _filterItemService.GetFilterItems(query.ObservationPredicate())
                .GroupBy(item => item.FilterGroup.Filter)
                .ToDictionary(
                    itemsGroupedByFilter => itemsGroupedByFilter.Key.Label.PascalCase(),
                    itemsGroupedByFilter => new LegendOptionsMetaValueModel<Dictionary<string, FilterItemMetaViewModel>>
                    {
                        Hint = itemsGroupedByFilter.Key.Hint,
                        Legend = itemsGroupedByFilter.Key.Label,
                        Options = itemsGroupedByFilter.GroupBy(item => item.FilterGroup).ToDictionary(
                            itemsGroupedByFilterGroup => itemsGroupedByFilterGroup.Key.Label.PascalCase(),
                            itemsGroupedByFilterGroup =>
                                new FilterItemMetaViewModel
                                {
                                    Label = itemsGroupedByFilterGroup.Key.Label,
                                    Options = itemsGroupedByFilterGroup.Select(item => new LabelValueViewModel
                                    {
                                        Label = item.Label,
                                        Value = item.Id.ToString()
                                    }),
                                    TotalValue =
                                        itemsGroupedByFilterGroup.FirstOrDefault(IsFilterItemTotal)?.Id.ToString() ??
                                        string.Empty
                                })
                    });
        }

        private static bool IsFilterItemTotal(FilterItem item)
        {
            return item.Label.Equals("Total", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}