using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IFilterItemRepository : IRepository<FilterItem, Guid>
    {
        IEnumerable<FilterItem> GetFilterItems(Guid subjectId, IQueryable<Observation> observations, bool listFilterItems);

        FilterItem GetTotal(Filter filter);

        FilterItem GetTotal(IEnumerable<FilterItem> filterItems);
    }
}