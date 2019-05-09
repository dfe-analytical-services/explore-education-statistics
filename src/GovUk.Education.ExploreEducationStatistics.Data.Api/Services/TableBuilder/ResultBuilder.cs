using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class ResultBuilder : IResultBuilder<Observation, TableBuilderObservationViewModel>
    {
        private readonly IMapper _mapper;

        public ResultBuilder(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TableBuilderObservationViewModel BuildResult(Observation observation, IEnumerable<long> indicators)
        {
            return new TableBuilderObservationViewModel
            {
                Filters = FilterItems(observation),
                Location = _mapper.Map<LocationViewModel>(observation.Location),
                Measures = Measures(observation, indicators),
                TimeIdentifier = observation.TimeIdentifier,
                Year = observation.Year
            };
        }

        private static IEnumerable<string> FilterItems(Observation observation)
        {
            return observation.FilterItems.Select(item => item.FilterItemId).OrderBy(l => l).Select(l => l.ToString());
        }

        private static Dictionary<string, string> Measures(Observation observation, IEnumerable<long> indicators)
        {
            var measures = indicators.Any()
                ? QueryUtil.FilterMeasures(observation.Measures, indicators)
                : observation.Measures;
            return measures.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value);
        }
    }
}