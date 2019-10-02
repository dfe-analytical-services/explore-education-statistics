using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectMetaService : AbstractTableBuilderSubjectMetaService, ISubjectMetaService
    {
        private readonly IBoundaryLevelService _boundaryLevelService;
        
        private readonly IGeoJsonService _geoJsonService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly ITimePeriodService _timePeriodService;
        private readonly ISubjectService _subjectService;
        private readonly IFootnoteService _footnoteService;
        private readonly IMapper _mapper;

        public SubjectMetaService(IBoundaryLevelService boundaryLevelService,
            IFilterItemService filterItemService,
            IGeoJsonService geoJsonService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IMapper mapper,
            ITimePeriodService timePeriodService,
            ISubjectService subjectService,
            IFootnoteService footnoteService
            ) : base(filterItemService)
        {
            _boundaryLevelService = boundaryLevelService;
            _geoJsonService = geoJsonService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _mapper = mapper;
            _timePeriodService = timePeriodService;
            _subjectService = subjectService;
            _footnoteService = footnoteService;
        }

        public SubjectMetaViewModel GetSubjectMeta(
            SubjectMetaQueryContext query,
            IQueryable<Observation> observations)
        {
            
            var subject = _subjectService.Find(query.SubjectId,
                new List<Expression<Func<Subject, object>>> {s => s.Release.Publication});
            if (subject == null)
            {
                throw new ArgumentException("Subject does not exist", nameof(query.SubjectId));
            }
            
            var observationalUnits = GetObservationalUnits(observations);
            return new SubjectMetaViewModel
            {
                Filters = GetFilters(observations),
                Indicators = GetIndicators(query),
                Locations = GetGeoJsonObservationalUnits(observationalUnits, query.BoundaryLevel),
                BoundaryLevels = GetBoundaryLevelOptions(query.BoundaryLevel, observationalUnits.Keys),
                PublicationName = subject.Release.Publication.Title,
                SubjectName = subject.Name,
                TimePeriods = GetTimePeriods(observations),
                Footnotes = GetFootnotes(observations, query),
            };
        }



        private Dictionary<string, IndicatorMetaViewModel> GetIndicators(SubjectMetaQueryContext query)
        {
            var indicatorList = _indicatorService.GetIndicators(query.SubjectId, query.Indicators);
            return indicatorList.ToDictionary(
                indicator => indicator.Id.ToString(),
                indicator => _mapper.Map<IndicatorMetaViewModel>(indicator));
        }

        private Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnits(
            IQueryable<Observation> observations)
        {
            return _locationService.GetObservationalUnits(observations);
        }

        private Dictionary<string, ObservationalUnitGeoJsonMeta> GetGeoJsonObservationalUnits(
            Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> observationalUnits,
            long? boundaryLevelId = null)
        {
            var observationalUnitMetaViewModels = observationalUnits.SelectMany(pair =>
                pair.Value.Select(observationalUnit => new ObservationalUnitGeoJsonMeta
                {
                    GeoJson = GetGeoJsonForObservationalUnit(boundaryLevelId ??
                                                             GetBoundaryLevel(pair.Key).Id, observationalUnit),
                    Label = observationalUnit.Name,
                    Level = pair.Key,
                    Value = observationalUnit.Code
                }));

            return observationalUnitMetaViewModels.ToDictionary(
                model => model.Value,
                model => model);
        }

        private BoundaryLevel GetBoundaryLevel(GeographicLevel geographicLevel)
        {
            return _boundaryLevelService.FindLatestByGeographicLevel(geographicLevel);
        }

        private IEnumerable<IdLabel> GetBoundaryLevelOptions(long? boundaryLevelId, IEnumerable<GeographicLevel> geographicLevels)
        {
            var boundaryLevels = boundaryLevelId.HasValue
                ? _boundaryLevelService.FindRelatedByBoundaryLevel(boundaryLevelId.Value)
                : _boundaryLevelService.FindByGeographicLevels(geographicLevels);
            return boundaryLevels.Select(level => _mapper.Map<IdLabel>(level));
        }

        private Dictionary<string, TimePeriodMetaViewModel> GetTimePeriods(IQueryable<Observation> observations)
        {
            return _timePeriodService.GetTimePeriods(observations).ToDictionary(
                tuple => tuple.GetTimePeriod(),
                tuple => new TimePeriodMetaViewModel(tuple.Year, tuple.TimeIdentifier));
        }

        private dynamic GetGeoJsonForObservationalUnit(long boundaryLevelId, IObservationalUnit observationalUnit)
        {
            var geoJson = _geoJsonService.Find(boundaryLevelId, observationalUnit.Code);
            return geoJson != null ? JsonConvert.DeserializeObject(geoJson.Value) : null;
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
        
        
    }
    
    
}