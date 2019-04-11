using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.TableBuilder
{
    public class ResultBuilder : IResultBuilder<Observation, TableBuilderObservation>
    {
        public TableBuilderObservation BuildResult(Observation observation)
        {
            return new TableBuilderObservation
            {
                Year = observation.Year,
                TimePeriod = observation.TimePeriod
            };
        }
    }
}