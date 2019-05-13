using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFilterItemService : IDataService<FilterItem, long>
    {
        IEnumerable<FilterItem> GetFilterItems(long subjectId,
            IEnumerable<int> years = null,
            IEnumerable<string> countries = null,
            IEnumerable<string> regions = null,
            IEnumerable<string> localAuthorities = null,
            IEnumerable<string> localAuthorityDistricts = null);
    }
}