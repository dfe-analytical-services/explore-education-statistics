#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IFilterRepository : IRepository<Filter, Guid>
    {
        List<Filter> GetFiltersIncludingItems(Guid subjectId);
    }
}
