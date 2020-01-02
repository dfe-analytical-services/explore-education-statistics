using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IFilterService : IRepository<Filter, Guid>
    {
        IEnumerable<Filter> GetFiltersIncludingItems(Guid subjectId);
    }
}