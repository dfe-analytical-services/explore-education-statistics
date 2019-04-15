using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class ResultBuilder : IResultBuilder<Observation, TableBuilderObservation>
    {
        private readonly IMapper _mapper;

        public ResultBuilder(IMapper mapper)
        {
            _mapper = mapper;
        }

        public TableBuilderObservation BuildResult(Observation observation, ICollection<string> indicators)
        {
            return new TableBuilderObservation
            {
                Location = _mapper.Map<LocationViewModel>(observation.Location),
                Measures = indicators.Count > 0
                    ? QueryUtil.FilterMeasures(observation.Measures, indicators)
                    : observation.Measures,
                TimePeriod = observation.TimePeriod,
                Year = observation.Year
            };
        }
    }
}