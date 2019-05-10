using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IIndicatorGroupService : IDataService<IndicatorGroup, long>
    {
        IEnumerable<IndicatorGroup> GetIndicatorGroups(long subjectId,
            IEnumerable<int> years = null,
            IEnumerable<string> countries = null,
            IEnumerable<string> regions = null,
            IEnumerable<string> localAuthorities = null,
            IEnumerable<string> localAuthorityDistricts = null);
    }
}