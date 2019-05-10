using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFilterService : IDataService<Filter, long>
    {
        IEnumerable<Filter> GetFilters(long subjectId,
            IEnumerable<int> years = null,
            IEnumerable<string> countries = null,
            IEnumerable<string> regions = null,
            IEnumerable<string> localAuthorities = null,
            IEnumerable<string> localAuthorityDistricts = null);
    }
}