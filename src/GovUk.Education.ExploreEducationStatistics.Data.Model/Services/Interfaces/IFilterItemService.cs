using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFilterItemService : IRepository<FilterItem, Guid>
    {
        IEnumerable<FilterItem> GetFilterItems(Guid subjectId, IQueryable<Observation> observations, bool listFilterItems);

        FilterItem GetTotal(Filter filter);
        
        FilterItem GetTotal(IEnumerable<FilterItem> filterItems);
    }
}