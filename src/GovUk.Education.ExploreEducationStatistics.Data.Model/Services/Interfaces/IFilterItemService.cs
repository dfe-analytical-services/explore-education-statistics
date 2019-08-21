using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFilterItemService : IRepository<FilterItem, long>
    {
        IEnumerable<FilterItem> GetFilterItems(IQueryable<Observation> observations);
        
        IEnumerable<FilterItem> GetFilterItemsIncludingFilters(IQueryable<Observation> observations);

        FilterItem GetTotal(IEnumerable<FilterItem> filterItems);
    }
}