using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFilterService : IRepository<Filter, long>
    {
        IEnumerable<Filter> GetFiltersIncludingItems(long subjectId);
    }
}