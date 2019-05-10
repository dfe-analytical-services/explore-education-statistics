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

        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnitsMeta(long subjectId,
            IEnumerable<int> years = null);

        IEnumerable<(TimeIdentifier TimePeriod, int Year)> GetTimePeriodsMeta(long subjectId,
            IEnumerable<int> years = null);
    }
}