#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IFilterItemRepository : IRepository<FilterItem, Guid>
    {
        Task<Dictionary<Guid, int>> CountFilterItemsByFilter(IEnumerable<Guid> filterItemIds);

        IEnumerable<FilterItem> GetFilterItems(
            Guid subjectId,
            IQueryable<Observation> observations,
            bool listFilterItems);

        FilterItem? GetTotal(Filter filter);

        FilterItem? GetTotal(IEnumerable<FilterItem> filterItems);
    }
}
