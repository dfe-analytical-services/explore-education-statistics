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
                // TODO add filters
                Filters = new Dictionary<long, string>(),
                Location = _mapper.Map<LocationViewModel>(observation.Location),
                Measures = indicators.Any()
                    ? QueryUtil.FilterMeasures(observation.Measures, indicators)
                    : observation.Measures,
                TimeIdentifier = observation.TimeIdentifier,
                Year = observation.Year
            };
        }
    }
}