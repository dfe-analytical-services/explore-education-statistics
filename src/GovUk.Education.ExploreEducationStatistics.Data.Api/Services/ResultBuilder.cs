using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services
{
    public class ResultBuilder : IResultBuilder<Observation, ObservationViewModel>
    {
        private readonly IMapper _mapper;

        public ResultBuilder(IMapper mapper)
        {
            _mapper = mapper;
        }

        public ObservationViewModel BuildResult(Observation observation, IEnumerable<string> indicators)
        {
            return new ObservationViewModel
            {
                Filters = FilterItems(observation),
                GeographicLevel = observation.GeographicLevel,
                Location = _mapper.Map<LocationViewModel>(observation.Location),
                Measures = Measures(observation, indicators),
                TimePeriod = observation.GetTimePeriod()
            };
        }

        private static IEnumerable<string> FilterItems(Observation observation)
        {
            return observation.FilterItems.Select(item => item.FilterItemId).OrderBy(l => l).Select(l => l.ToString());
        }

        private static Dictionary<string, string> Measures(Observation observation, IEnumerable<string> indicators)
        {
            var measures = indicators != null && indicators.Any()
                ? QueryUtil.FilterMeasures(observation.Measures, indicators)
                : observation.Measures;
            return measures.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);
        }
    }
}