using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IObservationService : IDataService<Observation, long>
    {
        IEnumerable<Observation> FindObservations(long subjectId,
            GeographicLevel geographicLevel,
            IEnumerable<int> years,
            IEnumerable<string> countries,
            IEnumerable<string> regions,
            IEnumerable<string> localAuthorities,
            IEnumerable<string> localAuthorityDistricts,
            IEnumerable<long> filters);

        Dictionary<GeographicLevel, IEnumerable<ObservationalUnit>> GetObservationalUnits(long subjectId);

        IEnumerable<(TimeIdentifier TimePeriod, int Year)> GetTimePeriodsMeta(long subjectId);
    }
}