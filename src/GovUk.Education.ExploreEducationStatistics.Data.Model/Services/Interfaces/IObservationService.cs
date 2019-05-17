using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IObservationService : IDataService<Observation, long>
    {
        IEnumerable<Observation> FindObservations(ObservationQueryContext query);

        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnitsMeta(
            SubjectMetaQueryContext query);

        IEnumerable<(TimeIdentifier TimePeriod, int Year)> GetTimePeriodsMeta(SubjectMetaQueryContext query);
    }
}