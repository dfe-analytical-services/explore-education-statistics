using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

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
            IEnumerable<string> localEnterprisePartnerships,
            IEnumerable<string> institutions,
            IEnumerable<string> mats,
            IEnumerable<string> mayoralCombinedAuthorities,
            IEnumerable<string> opportunityAreas,
            IEnumerable<string> parliamentaryConstituencies,
            IEnumerable<string> providers,
            IEnumerable<string> wards,
            IEnumerable<long> filters);

        Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>> GetObservationalUnitsMeta(
            SubjectMetaQueryContext query);

        IEnumerable<(TimeIdentifier TimePeriod, int Year)> GetTimePeriodsMeta(SubjectMetaQueryContext query);
    }
}