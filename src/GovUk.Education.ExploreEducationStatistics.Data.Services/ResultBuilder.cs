using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services
{
    public class ResultBuilder : IResultBuilder<Observation, ObservationViewModel>
    {
        private readonly IMapper _mapper;

        public ResultBuilder(IMapper mapper)
        {
            _mapper = mapper;
        }

        public ObservationViewModel BuildResult(Observation observation, IEnumerable<long> indicators)
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

        private static Dictionary<string, string> Measures(Observation observation, IEnumerable<long> indicators)
        {
            var indicatorsList = indicators?.ToList();
            var measures = indicatorsList != null && indicatorsList.Any()
                ? FilterMeasures(observation.Measures, indicatorsList)
                : observation.Measures;
            return measures.Where(pair => pair.Value != null)
                .ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);
        }
        
        private static Dictionary<long, string> FilterMeasures(
            Dictionary<long, string> measures,
            IEnumerable<long> indicators)
        {
            return (
                from kvp in measures
                where indicators.Contains(kvp.Key)
                select kvp
            ).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}