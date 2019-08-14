using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore.Internal;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class TableBuilderResultSubjectMetaService : ITableBuilderResultSubjectMetaService
    {
        private readonly IFilterItemService _filterItemService;
        private readonly IFootnoteService _footnoteService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly IMapper _mapper;
        private readonly ISubjectService _subjectService;
        private readonly ITimePeriodService _timePeriodService;

        public TableBuilderResultSubjectMetaService(
            IFootnoteService footnoteService,
            IFilterItemService filterItemService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IMapper mapper,
            ISubjectService subjectService,
            ITimePeriodService timePeriodService)
        {
            _filterItemService = filterItemService;
            _footnoteService = footnoteService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _mapper = mapper;
            _subjectService = subjectService;
            _timePeriodService = timePeriodService;
        }

        public TableBuilderResultSubjectMetaViewModel GetSubjectMeta(SubjectMetaQueryContext query,
            IQueryable<Observation> observations)
        {
            var subject = _subjectService.Find(query.SubjectId,
                new List<Expression<Func<Subject, object>>> {s => s.Release.Publication});
            if (subject == null)
            {
                throw new ArgumentException("Subject does not exist", nameof(query.SubjectId));
            }

            return new TableBuilderResultSubjectMetaViewModel
            {
                Filters = GetFilters(observations),
                Footnotes = GetFootnotes(observations, query),
                Indicators = GetIndicators(query),
                Locations = GetObservationalUnits(observations),
                PublicationName = subject.Release.Publication.Title,
                SubjectName = subject.Name,
                TimePeriodRange = GetTimePeriodRange(observations)
            };
        }

        private IEnumerable<LabelValue> GetObservationalUnits(IQueryable<Observation> observations)
        {
            var observationalUnits = _locationService.GetObservationalUnits(observations);
            return observationalUnits.SelectMany(pair => pair.Value.Select(observationalUnit => new LabelValue
            {
                Label = observationalUnit.Name,
                Value = observationalUnit.Code
            }));
        }

        private IEnumerable<IndicatorMetaViewModel> GetIndicators(SubjectMetaQueryContext query)
        {
            return _mapper.Map<IEnumerable<IndicatorMetaViewModel>>(
                _indicatorService.GetIndicators(query.SubjectId, query.Indicators));
        }

        private IEnumerable<TableBuilderResultFilterItemMetaViewModel> GetFilters(IQueryable<Observation> observations)
        {
            var filterItems = _filterItemService.GetFilterItemsIncludingFilters(observations).ToList();
            var filterItemsGroupedByFilter = filterItems.GroupBy(item => item.FilterGroup.Filter.Id);
            var totalsByFilter = filterItemsGroupedByFilter.ToDictionary(items => items.Key, 
                items => _filterItemService.GetTotal(items)?.Id);
            return filterItems.Select(item =>
            {
                var isTotal = totalsByFilter[item.FilterGroup.Filter.Id] == item.Id;
                return new TableBuilderResultFilterItemMetaViewModel
                {
                    IsTotal = isTotal,
                    Label = item.Label,
                    Value = item.Id.ToString()
                };
            });
        }

        private IEnumerable<FootnoteViewModel> GetFootnotes(IQueryable<Observation> observations,
            SubjectMetaQueryContext queryContext)
        {
            return _footnoteService.GetFootnotes(queryContext.SubjectId, observations, queryContext.Indicators)
                .Select(footnote => new FootnoteViewModel
                {
                    Id = footnote.Id,
                    Label = footnote.Content
                });
        }

        private IEnumerable<TimePeriodMetaViewModel> GetTimePeriodRange(IQueryable<Observation> observations)
        {
            return _timePeriodService.GetTimePeriodRange(observations).Select(tuple =>
                new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));
        }
    }
}