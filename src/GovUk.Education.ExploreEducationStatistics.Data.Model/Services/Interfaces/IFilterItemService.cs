using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFilterItemService : IDataService<FilterItem, long>
    {
        IEnumerable<FilterItem> GetFilterItems(SubjectMetaQueryContext query);
    }
}