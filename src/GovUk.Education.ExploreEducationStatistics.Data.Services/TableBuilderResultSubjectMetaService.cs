using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class TableBuilderResultSubjectMetaService : AbstractTableBuilderSubjectMetaService,
        ITableBuilderResultSubjectMetaService
    {
        private readonly IFootnoteService _footnoteService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ISubjectService _subjectService;
        private readonly ITimePeriodService _timePeriodService;

        public TableBuilderResultSubjectMetaService(IFilterItemService filterItemService,
            IFootnoteService footnoteService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            ILogger<TableBuilderResultSubjectMetaService> logger,
            IMapper mapper,
            ISubjectService subjectService,
            ITimePeriodService timePeriodService) : base(filterItemService)
        {
            _footnoteService = footnoteService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _logger = logger;
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

            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();

            var filters = GetFilters2(observations);

            _logger.LogTrace("Got Filters in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();

            var footnotes = GetFootnotes(observations, query);

            _logger.LogTrace("Got Footnotes in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();

            var indicators = GetIndicators(query);

            _logger.LogTrace("Got Indicators in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();

            var locations = GetObservationalUnits(observations);

            _logger.LogTrace("Got Observational Units in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Restart();

            var timePeriodRange = GetTimePeriodRange(observations);

            _logger.LogTrace("Got Time Periods in {Time} ms", stopwatch.Elapsed.TotalMilliseconds);
            stopwatch.Stop();

            return new TableBuilderResultSubjectMetaViewModel
            {
                Filters = filters,
                Footnotes = footnotes,
                Indicators = indicators,
                Locations = locations,
                PublicationName = subject.Release.Publication.Title,
                SubjectName = subject.Name,
                TimePeriodRange = timePeriodRange
            };
        }

        private IEnumerable<ObservationalUnitMetaViewModel> GetObservationalUnits(IQueryable<Observation> observations)
        {
            var observationalUnits = _locationService.GetObservationalUnits(observations);

            var viewModels = observationalUnits.SelectMany(pair => pair.Value.Select(observationalUnit =>
                new ObservationalUnitMetaViewModel
                {
                    Label = observationalUnit.Name,
                    Level = pair.Key,
                    Value = observationalUnit.Code
                }));

            return TransformDuplicateObservationalUnitsWithUniqueLabels(viewModels);
        }

        private IEnumerable<IndicatorMetaViewModel> GetIndicators(SubjectMetaQueryContext query)
        {
            return _mapper.Map<IEnumerable<IndicatorMetaViewModel>>(
                _indicatorService.GetIndicators(query.SubjectId, query.Indicators));
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