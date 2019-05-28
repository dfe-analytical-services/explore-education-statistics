using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class SubjectMetaService : ISubjectMetaService
    {
        private readonly IGeoJsonService _geoJsonService;
        private readonly IIndicatorService _indicatorService;
        private readonly ILocationService _locationService;
        private readonly IMapper _mapper;
        private readonly ISubjectService _subjectService;

        public SubjectMetaService(IGeoJsonService geoJsonService,
            IIndicatorService indicatorService,
            ILocationService locationService,
            IMapper mapper,
            ISubjectService subjectService)
        {
            _geoJsonService = geoJsonService;
            _indicatorService = indicatorService;
            _locationService = locationService;
            _mapper = mapper;
            _subjectService = subjectService;
        }

        public SubjectMetaViewModel GetSubjectMeta(
            SubjectMetaQueryContext query,
            IEnumerable<Observation> observations)
        {
            var subject = _subjectService.Find(query.SubjectId);
            if (subject == null)
            {
                throw new ArgumentException("Subject does not exist", nameof(query.SubjectId));
            }

            return new SubjectMetaViewModel
            {
                Filters = GetFilters(observations),
                Indicators = GetIndicators(subject.Id, query.Indicators),
                Locations = GetObservationalUnits(observations)
            };
        }

        private static Dictionary<string, LabelValueViewModel> GetFilters(IEnumerable<Observation> observations)
        {
            var filterItems = observations.SelectMany(observation => observation.FilterItems)
                .Select(item => item.FilterItem).Distinct();

            return filterItems.ToDictionary(
                item => item.Id.ToString(),
                item => new LabelValueViewModel
                {
                    Label = item.Label,
                    Value = item.Id.ToString()
                });
        }

        private Dictionary<string, IndicatorMetaViewModel> GetIndicators(long subjectId, IEnumerable<long> indicators)
        {
            var indicatorList = indicators == null || !indicators.Any()
                ? _indicatorService.GetIndicators(subjectId)
                : _indicatorService.GetIndicators(subjectId, indicators);

            return indicatorList.ToDictionary(
                indicator => indicator.Id.ToString(),
                indicator => _mapper.Map<IndicatorMetaViewModel>(indicator));
        }

        private Dictionary<string, ObservationalUnitMetaViewModel> GetObservationalUnits(
            IEnumerable<Observation> observations)
        {
            var locations = observations
                .GroupBy(observation => observation.Location)
                .Select(group => group.Key);

            var observationalUnits =
                _locationService.GetObservationalUnits(locations).Values.SelectMany(units => units);

            // TODO should we just be extracting the most significant observational unit for the query here?

            return observationalUnits.ToDictionary(
                observationalUnit => observationalUnit.Code,
                observationalUnit => new ObservationalUnitMetaViewModel
                {
                    GeoJson = GetGeoJsonForObservationalUnit(observationalUnit),
                    Label = observationalUnit.Name,
                    Value = observationalUnit.Code
                });
        }

        private dynamic GetGeoJsonForObservationalUnit(IObservationalUnit observationalUnit)
        {
            return JsonConvert.DeserializeObject(_geoJsonService.Find(observationalUnit.Code).Value);
        }
    }
}