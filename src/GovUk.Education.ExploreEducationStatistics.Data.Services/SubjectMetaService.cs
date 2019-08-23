using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class SubjectMetaService : ISubjectMetaService
    {
        private readonly IBoundaryLevelService _boundaryLevelService;
        private readonly IFilterItemService _filterItemService;
        private readonly IGeoJsonService _geoJsonService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly ITimePeriodService _timePeriodService;
        private readonly IMapper _mapper;

        public SubjectMetaService(IBoundaryLevelService boundaryLevelService,
            IFilterItemService filterItemService,
            IGeoJsonService geoJsonService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IMapper mapper,
            ITimePeriodService timePeriodService)
        {
            _boundaryLevelService = boundaryLevelService;
            _filterItemService = filterItemService;
            _geoJsonService = geoJsonService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _mapper = mapper;
            _timePeriodService = timePeriodService;
        }

        public SubjectMetaViewModel GetSubjectMeta(
            SubjectMetaQueryContext query,
            IQueryable<Observation> observations)
        {
            var observationalUnits = GetObservationalUnits(observations);
            return new SubjectMetaViewModel
            {
                Filters = GetFilters(observations),
                Indicators = GetIndicators(query),
                Locations = GetGeoJsonObservationalUnits(observationalUnits, query.BoundaryLevel),
                BoundaryLevels = GetBoundaryLevelOptions(query.BoundaryLevel, observationalUnits.Keys),
                TimePeriods = GetTimePeriods(observations)
            };
        }

        private Dictionary<string, LabelValue> GetFilters(IQueryable<Observation> observations)
        {
            return _filterItemService.GetFilterItems(observations).ToDictionary(
                item => item.Id.ToString(),
                item => new LabelValue
                {
                    Label = item.Label,
                    Value = item.Id.ToString()
                });
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
    }
}