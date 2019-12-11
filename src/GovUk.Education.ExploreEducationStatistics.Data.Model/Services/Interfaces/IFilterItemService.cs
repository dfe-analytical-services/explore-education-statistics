using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFilterItemService : IRepository<FilterItem, Guid>
    {
        IEnumerable<FilterItem> GetFilterItemsIncludingFilters(IQueryable<Observation> observations);

        FilterItem GetTotal(Filter filter);
        
        FilterItem GetTotal(IEnumerable<FilterItem> filterItems);
    }
}